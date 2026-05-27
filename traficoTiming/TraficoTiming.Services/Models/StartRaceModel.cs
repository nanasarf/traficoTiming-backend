using TraficoTiming.Database.Enums;

namespace TraficoTiming.Services.Models;

public class StartRaceModel
{
    public Guid TimingSessionId { get; set; }
    public Guid StarterDeviceId { get; set; }
    public StartMethod StartMethod { get; set; }
    public bool GunSoundPlayed { get; set; }
    public bool FlashTriggered { get; set; }
}
