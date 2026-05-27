namespace TraficoTiming.Api.Models.Requests;

public class ClockSyncRequest
{
    public Guid StarterDeviceId { get; set; }
    public Guid FinishDeviceId { get; set; }

    public decimal OffsetMs { get; set; }
    public decimal RoundTripDelayMs { get; set; }
    public decimal? DriftMs { get; set; }
    public decimal SyncQualityScore { get; set; }
}
