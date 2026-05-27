using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Interfaces;

public interface IDevicePairingService
{
    Task<TimingDevice> PairDeviceAsync(PairDeviceModel model, CancellationToken ct = default);
    Task<IReadOnlyList<TimingDevice>> GetSessionDevicesAsync(Guid sessionId, CancellationToken ct = default);
    Task UpdateHeartbeatAsync(Guid deviceId, double? batteryLevel, CancellationToken ct = default);
    Task DisconnectDeviceAsync(Guid deviceId, CancellationToken ct = default);
}
