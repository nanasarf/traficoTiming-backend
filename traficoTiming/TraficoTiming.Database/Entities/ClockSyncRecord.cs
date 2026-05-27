namespace TraficoTiming.Database.Entities;

public class ClockSyncRecord
{
    public Guid Id { get; set; }

    public Guid TimingSessionId { get; set; }
    public TimingSession TimingSession { get; set; } = null!;

    public Guid StarterDeviceId { get; set; }
    public Guid FinishDeviceId { get; set; }

    public decimal OffsetMs { get; set; }
    public decimal RoundTripDelayMs { get; set; }
    public decimal? DriftMs { get; set; }

    public decimal SyncQualityScore { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
