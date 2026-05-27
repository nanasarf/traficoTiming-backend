using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;

namespace TraficoTiming.Repository.Interfaces;

public interface ITimingDeviceRepository
{
    Task<TimingDevice> CreateAsync(TimingDevice device, CancellationToken ct = default);
    Task<TimingDevice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TimingDevice>> GetDevicesBySessionAsync(Guid sessionId, CancellationToken ct = default);
    Task<TimingDevice?> GetStarterDeviceAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<TimingDevice>> GetFinishDevicesAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<TimingDevice>> GetConnectedDevicesAsync(Guid sessionId, CancellationToken ct = default);
    Task<bool> RoleExistsInSessionAsync(Guid sessionId, DeviceRole role, CancellationToken ct = default);
    Task<bool> DeviceNameExistsInSessionAsync(Guid sessionId, string deviceName, CancellationToken ct = default);
    Task UpdateAsync(TimingDevice device, CancellationToken ct = default);
    Task DeleteAsync(TimingDevice device, CancellationToken ct = default);
}
