using TraficoTiming.Database.Enums;

namespace TraficoTiming.Api.Models.Responses;

public class RaceStartResponse
{
    public Guid Id { get; set; }
    public Guid TimingSessionId { get; set; }
    public Guid StarterDeviceId { get; set; }
    public DateTime StartTimestampUtc { get; set; }
    public StartMethod StartMethod { get; set; }
    public bool GunSoundPlayed { get; set; }
    public bool FlashTriggered { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
