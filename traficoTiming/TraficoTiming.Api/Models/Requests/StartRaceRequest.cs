using TraficoTiming.Database.Enums;

namespace TraficoTiming.Api.Models.Requests;

public class StartRaceRequest
{
    public Guid StarterDeviceId { get; set; }

    public DateTime StartTimestampUtc { get; set; }

    public StartMethod StartMethod { get; set; } = StartMethod.GunButton;

    public bool GunSoundPlayed { get; set; }
    public bool FlashTriggered { get; set; }
}

