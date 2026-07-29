using TraficoTiming.Database.Enums;

namespace TraficoTiming.Services.Models;

public class PairDeviceModel
{
    public Guid TimingSessionId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public DeviceRole DeviceRole { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public ConnectionType ConnectionType { get; set; }
    public int? BatteryLevel { get; set; }
}
