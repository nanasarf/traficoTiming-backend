using TraficoTiming.Database.Enums;

namespace TraficoTiming.Database.Entities;

public class TimingDevice
{
    public Guid Id { get; set; }

    public Guid TimingSessionId { get; set; }
    public TimingSession TimingSession { get; set; } = null!;

    public string DeviceName { get; set; } = string.Empty;

    public DeviceRole DeviceRole { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public ConnectionType ConnectionType { get; set; }

    public decimal? ClockOffsetMs { get; set; }
    public decimal? ClockDriftMs { get; set; }

    public DateTime? LastSyncedAtUtc { get; set; }

    public int? BatteryLevel { get; set; }

    public DeviceStatus Status { get; set; } = DeviceStatus.Connected;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
