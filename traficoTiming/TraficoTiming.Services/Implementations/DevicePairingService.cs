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
            BatteryLevel    = model.BatteryLevel,
            Status          = DeviceStatus.Connected
        };
        await deviceRepo.CreateAsync(device, ct);

        var hasStarter = await deviceRepo.RoleExistsInSessionAsync(model.TimingSessionId, DeviceRole.Starter, ct);
        var hasFinish  = await deviceRepo.RoleExistsInSessionAsync(model.TimingSessionId, DeviceRole.Finish, ct);
        SessionStatus? previousStatus = null;
        SessionStatus? newStatus = null;
        string? reason = null;
        if (hasStarter && hasFinish && session.Status == SessionStatus.Created)
        {
            previousStatus = session.Status;
            newStatus = SessionStatus.DevicesPaired;
            reason = "second required device paired";
            await sessionRepo.UpdateStatusAsync(model.TimingSessionId, SessionStatus.DevicesPaired, ct);
        }
        await auditLog.LogAsync(model.TimingSessionId, AuditActions.DevicePaired, device.Id,
            details: new
            {
                deviceRole = device.DeviceRole.ToString(),
                connectionType = device.ConnectionType.ToString(),
                previousStatus = previousStatus?.ToString(),
                newStatus = newStatus?.ToString(),
                reason
            }, ct: ct);

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
        await auditLog.LogAsync(device.TimingSessionId, AuditActions.DeviceHeartbeatUpdated, device.Id,
            details: new { batteryLevel = device.BatteryLevel }, ct: ct);
    }

    public async Task DisconnectDeviceAsync(Guid deviceId, CancellationToken ct = default)
    {
        var device = await deviceRepo.GetByIdAsync(deviceId, ct)
            ?? throw new KeyNotFoundException($"Device {deviceId} not found.");
        device.Status = DeviceStatus.Disconnected;
        await deviceRepo.UpdateAsync(device, ct);

        var session = await sessionRepo.GetByIdAsync(device.TimingSessionId, ct);
        if (session is null || session.Status == SessionStatus.Running)
        {
            await LogDisconnectedAsync(device, null, null, null, ct);
            return;
        }

        if (session.Status is not (SessionStatus.DevicesPaired or SessionStatus.Ready))
        {
            await LogDisconnectedAsync(device, null, null, null, ct);
            return;
        }

        var devices = await deviceRepo.GetDevicesBySessionAsync(device.TimingSessionId, ct);
        var hasStarter = devices.Any(d => d.DeviceRole == DeviceRole.Starter);
        var hasFinish = devices.Any(d => d.DeviceRole == DeviceRole.Finish);
        var hasConnectedStarter = devices.Any(d =>
            d.DeviceRole == DeviceRole.Starter && d.Status == DeviceStatus.Connected);
        var hasConnectedFinish = devices.Any(d =>
            d.DeviceRole == DeviceRole.Finish && d.Status == DeviceStatus.Connected);

        SessionStatus? previousStatus = null;
        SessionStatus? newStatus = null;
        string? reason = null;
        if (session.Status == SessionStatus.Ready && (!hasConnectedStarter || !hasConnectedFinish))
        {
            previousStatus = session.Status;
            session.Status = hasStarter && hasFinish
                ? SessionStatus.DevicesPaired
                : SessionStatus.Created;
            newStatus = session.Status;
            reason = "required device disconnected";
            await sessionRepo.UpdateAsync(session, ct);
        }
        await LogDisconnectedAsync(device, previousStatus, newStatus, reason, ct);
    }

    private Task LogDisconnectedAsync(
        TimingDevice device,
        SessionStatus? previousStatus,
        SessionStatus? newStatus,
        string? reason,
        CancellationToken ct) =>
        auditLog.LogAsync(device.TimingSessionId, AuditActions.DeviceDisconnected, device.Id,
            details: new
            {
                deviceRole = device.DeviceRole.ToString(),
                previousStatus = previousStatus?.ToString(),
                newStatus = newStatus?.ToString(),
                reason
            }, ct: ct);
}
