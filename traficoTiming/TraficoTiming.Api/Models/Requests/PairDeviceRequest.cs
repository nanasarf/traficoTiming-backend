using TraficoTiming.Database.Enums;

namespace TraficoTiming.Api.Models.Requests;

public class PairDeviceRequest
{
    public string DeviceName { get; set; } = string.Empty;
    public DeviceRole DeviceRole { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public ConnectionType ConnectionType { get; set; }
    public int? BatteryLevel { get; set; }
}
