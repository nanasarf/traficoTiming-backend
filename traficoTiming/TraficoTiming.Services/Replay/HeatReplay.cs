namespace TraficoTiming.Services.Replay;

// Microseconds on each device's monotonic clock. Anchors must bracket the evidence.
public sealed record Anchor(long LocalUs, long StarterUs);
public sealed record Frame(long LocalUs, string File);
public sealed record Camera(string Id, Anchor First, Anchor Last, Frame[] Frames);
public sealed record Entry(int Lane, string Name);
public sealed record Mark(int Lane, string CameraId, int Before, int After);
public sealed record Heat(int Version, string Title, long StartUs, Camera[] Cameras, Entry[] Roster, Mark[] Marks);
public sealed record ReviewedLane(int Lane, string Name, long? EarliestUs, long? LatestUs);

public static class HeatReplay
{
    // JSON numbers must round-trip exactly through the browser.
    private const long MaxClock = 9007199254740991;
    private static bool IsClock(long value) => value is >= 0 and <= MaxClock;

    public static ReviewedLane[] Evaluate(Heat heat)
    {
        if (heat.Version != 1 || string.IsNullOrWhiteSpace(heat.Title) || !IsClock(heat.StartUs))
            throw new ArgumentException("Expected a named version 1 heat and a valid starter timestamp.");
        if (heat.Cameras is not { Length: 2 } || heat.Roster is not { Length: >= 1 and <= 9 }
            || heat.Marks is null || heat.Marks.Length > heat.Roster.Length)
            throw new ArgumentException("Supply two cameras, one to nine athletes, and at most one review per lane.");

        var cameras = new Dictionary<string, Camera>();
        foreach (var camera in heat.Cameras)
        {
            if (camera is null || string.IsNullOrWhiteSpace(camera.Id) || !cameras.TryAdd(camera.Id, camera)
                || camera.First is null || camera.Last is null || camera.Frames is not { Length: >= 2 })
                throw new ArgumentException("Invalid camera or frame index.");
            var first = camera.First;
            var last = camera.Last;
            if (!IsClock(first.LocalUs) || !IsClock(last.LocalUs) || !IsClock(first.StarterUs) || !IsClock(last.StarterUs)
                || first.LocalUs >= last.LocalUs || first.StarterUs >= last.StarterUs)
                throw new ArgumentException("Clock anchors must increase in both domains.");
            long previous = -1;
            foreach (var frame in camera.Frames)
            {
                if (frame is null || string.IsNullOrWhiteSpace(frame.File) || frame.LocalUs <= previous
                    || frame.LocalUs < first.LocalUs || frame.LocalUs > last.LocalUs)
                    throw new ArgumentException("Frame timestamps must increase within calibration coverage.");
                previous = frame.LocalUs;
            }
        }

        var lanes = new Dictionary<int, ReviewedLane>();
        foreach (var entry in heat.Roster)
        {
            if (entry is null || entry.Lane is < 1 or > 9 || string.IsNullOrWhiteSpace(entry.Name)
                || !lanes.TryAdd(entry.Lane, new(entry.Lane, entry.Name, null, null)))
                throw new ArgumentException("Each athlete needs a unique lane and a name.");
        }

        foreach (var mark in heat.Marks)
        {
            if (mark is null || !lanes.TryGetValue(mark.Lane, out var lane) || lane.EarliestUs.HasValue
                || mark.CameraId is null || !cameras.TryGetValue(mark.CameraId, out var camera)
                || mark.Before < 0 || mark.After <= mark.Before || mark.After >= camera.Frames.Length)
                throw new ArgumentException("Invalid or duplicate lane crossing.");
            var lower = ToStarter(camera, mark.Before, roundUp: false) - heat.StartUs;
            var upper = ToStarter(camera, mark.After, roundUp: true) - heat.StartUs;
            if (lower < 0)
                throw new ArgumentException("Crossing precedes the start.");
            lanes[mark.Lane] = lane with { EarliestUs = lower, LatestUs = upper };
        }
        return lanes.Values.OrderBy(lane => lane.Lane).ToArray();
    }

    private static long ToStarter(Camera camera, int index, bool roundUp)
    {
        long duration = camera.Last.LocalUs - camera.First.LocalUs;
        Int128 numerator = (Int128)(camera.Frames[index].LocalUs - camera.First.LocalUs)
            * (camera.Last.StarterUs - camera.First.StarterUs);
        // Divide only once so the frame bounds remain exact at large clock epochs.
        Int128 offset = (numerator + (roundUp ? duration - 1 : 0)) / duration;
        return camera.First.StarterUs + (long)offset;
    }
}
