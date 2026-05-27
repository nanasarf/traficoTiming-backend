namespace TraficoTiming.Api.Models.Responses;

public class ClockSyncResponse
{
    public Guid Id { get; set; }
    public Guid TimingSessionId { get; set; }
    public Guid StarterDeviceId { get; set; }
    public Guid FinishDeviceId { get; set; }
    public decimal OffsetMs { get; set; }
    public decimal RoundTripDelayMs { get; set; }
    public decimal? DriftMs { get; set; }
    public decimal SyncQualityScore { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
