namespace TraficoTiming.Services.Models;

public class ClockSyncModel
{
    public Guid TimingSessionId { get; set; }
    public Guid StarterDeviceId { get; set; }
    public Guid FinishDeviceId { get; set; }
    public long OffsetMs { get; set; }
    public long RoundTripDelayMs { get; set; }
    public long DriftMs { get; set; }
}
