using TraficoTiming.Database.Enums;

namespace TraficoTiming.Database.Entities;

public class RaceStartEvent
{
    public Guid Id { get; set; }

    public Guid TimingSessionId { get; set; }
    public TimingSession TimingSession { get; set; } = null!;

    public Guid StarterDeviceId { get; set; }

    public DateTime StartTimestampUtc { get; set; }

    public StartMethod StartMethod { get; set; } = StartMethod.GunButton;

    public bool GunSoundPlayed { get; set; }
    public bool FlashTriggered { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
