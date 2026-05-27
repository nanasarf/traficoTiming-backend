using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Implementations;

public class DevicePairingService(
    ITimingDeviceRepository deviceRepo,
    ITimingSessionRepository sessionRepo,
    IAuditLogService auditLog) : IDevicePairingService
{
    public async Task<TimingDevice> PairDeviceAsync(PairDeviceModel model, CancellationToken ct = default)
    {
        var session = await sessionRepo.GetByIdAsync(model.TimingSessionId, ct)
            ?? throw new KeyNotFoundException($"Session {model.TimingSessionId} not found.");

        if (session.Status is SessionStatus.Running or SessionStatus.Completed or SessionStatus.Cancelled)
            throw new InvalidOperationException($"Cannot pair devices to a session in {session.Status} status.");

        if (model.DeviceRole == DeviceRole.Starter && await deviceRepo.RoleExistsInSessionAsync(model.TimingSessionId, DeviceRole.Starter, ct))
            throw new InvalidOperationException("A Starter device is already paired to this session.");

        if (await deviceRepo.DeviceNameExistsInSessionAsync(model.TimingSessionId, model.DeviceName, ct))
            throw new InvalidOperationException($"A device named '{model.DeviceName}' is already paired to this session.");

        var device = new TimingDevice
        {
            TimingSessionId = model.TimingSessionId,
            DeviceName      = model.DeviceName,
            DeviceRole      = model.DeviceRole,
            DeviceType      = model.DeviceType,
            ConnectionType  = model.ConnectionType,
            Status          = DeviceStatus.Connected
        };
        await deviceRepo.CreateAsync(device, ct);

        var hasStarter = await deviceRepo.RoleExistsInSessionAsync(model.TimingSessionId, DeviceRole.Starter, ct);
        var hasFinish  = await deviceRepo.RoleExistsInSessionAsync(model.TimingSessionId, DeviceRole.Finish, ct);
        if (hasStarter && hasFinish && session.Status == SessionStatus.Created)
            await sessionRepo.UpdateStatusAsync(model.TimingSessionId, SessionStatus.DevicesPaired, ct);

        return device;
    }

    public async Task<IReadOnlyList<TimingDevice>> GetSessionDevicesAsync(Guid sessionId, CancellationToken ct = default) =>
        (await deviceRepo.GetDevicesBySessionAsync(sessionId, ct)).AsReadOnly();

    public async Task UpdateHeartbeatAsync(Guid deviceId, double? batteryLevel, CancellationToken ct = default)
    {
        var device = await deviceRepo.GetByIdAsync(deviceId, ct)
            ?? throw new KeyNotFoundException($"Device {deviceId} not found.");
        device.LastSyncedAtUtc = DateTime.UtcNow;
        device.BatteryLevel    = (int?)batteryLevel;
        await deviceRepo.UpdateAsync(device, ct);
    }

    public async Task DisconnectDeviceAsync(Guid deviceId, CancellationToken ct = default)
    {
        var device = await deviceRepo.GetByIdAsync(deviceId, ct)
            ?? throw new KeyNotFoundException($"Device {deviceId} not found.");
        device.Status = DeviceStatus.Disconnected;
        await deviceRepo.UpdateAsync(device, ct);
    }
}
