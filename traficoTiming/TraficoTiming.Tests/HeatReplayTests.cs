using TraficoTiming.Services.Replay;
using Xunit;

namespace TraficoTiming.Tests;

public class HeatReplayTests
{
    private static Heat Example()
    {
        Camera camera = new("A", new(100, 500), new(200, 701),
            [new(110, "a.png"), new(150, "b.png"), new(170, "c.png")]);
        return new(1, "100 m", 500, [camera, camera with { Id = "B" }],
            [new(1, "First"), new(2, "Second")], [new(2, "B", 0, 2)]);
    }

    [Fact]
    public void MapsDriftAndPreservesUnresolvedLaneAndMissingFrameGap()
    {
        var rows = HeatReplay.Evaluate(Example());
        Assert.Null(rows[0].EarliestUs);
        Assert.Equal(20L, rows[1].EarliestUs);
        Assert.Equal(141L, rows[1].LatestUs);
        Assert.Equal(rows, HeatReplay.Evaluate(Example()));
    }

    [Fact]
    public void TwoAthletesCanShareAFrameBracket()
    {
        var heat = Example() with { Marks = [new(1, "A", 0, 1), new(2, "A", 0, 1)] };
        var rows = HeatReplay.Evaluate(heat);
        Assert.Equal(rows[0].LatestUs, rows[1].LatestUs);
    }

    [Fact]
    public void RejectsDuplicateLaneDecisionsAndExpiredCalibration()
    {
        var heat = Example();
        Assert.Throws<ArgumentException>(() => HeatReplay.Evaluate(heat with { Marks = [heat.Marks[0], heat.Marks[0]] }));
        var bad = heat.Cameras[0] with { Last = new(140, 701) };
        Assert.Throws<ArgumentException>(() => HeatReplay.Evaluate(heat with { Cameras = [bad, heat.Cameras[1]] }));
    }
}
