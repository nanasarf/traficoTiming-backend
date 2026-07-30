using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Implementations;

public class ClockSyncService(
    IClockSyncRecordRepository clockSyncRepo,
    ITimingSessionRepository sessionRepo,
    ITimingDeviceRepository deviceRepo,
    IAuditLogService auditLog) : IClockSyncService
{
    public const decimal ReadinessThreshold = 70m;

    public async Task<ClockSyncRecord> SyncClocksAsync(ClockSyncModel model, CancellationToken ct = default)
    {
        if (model.SyncQualityScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(model.SyncQualityScore), "SyncQualityScore must be between 0 and 100.");

        if (model.StarterDeviceId == model.FinishDeviceId)
            throw new ArgumentException("Starter and Finish device IDs must be different.", nameof(model));

        var session = await sessionRepo.GetByIdAsync(model.TimingSessionId, ct)
            ?? throw new KeyNotFoundException($"Session {model.TimingSessionId} not found.");

        if (session.Status is not (SessionStatus.Created or SessionStatus.DevicesPaired or SessionStatus.Ready))
            throw new InvalidOperationException($"Clock synchronization is not allowed while the session is {session.Status}.");

        var starter = await deviceRepo.GetByIdAsync(model.StarterDeviceId, ct)
            ?? throw new KeyNotFoundException($"Device {model.StarterDeviceId} not found.");
        var finish = await deviceRepo.GetByIdAsync(model.FinishDeviceId, ct)
            ?? throw new KeyNotFoundException($"Device {model.FinishDeviceId} not found.");

        ValidateDevice(starter, model.TimingSessionId, DeviceRole.Starter);
        ValidateDevice(finish, model.TimingSessionId, DeviceRole.Finish);

        var record = new ClockSyncRecord
        {
            TimingSessionId  = model.TimingSessionId,
            StarterDeviceId  = model.StarterDeviceId,
            FinishDeviceId   = model.FinishDeviceId,
            OffsetMs         = model.OffsetMs,
            RoundTripDelayMs = model.RoundTripDelayMs,
            DriftMs          = model.DriftMs,
            SyncQualityScore = model.SyncQualityScore
        };

        var syncedAtUtc = DateTime.UtcNow;
        starter.ClockOffsetMs = 0;
        starter.ClockDriftMs = 0;
        starter.LastSyncedAtUtc = syncedAtUtc;
        finish.ClockOffsetMs = model.OffsetMs;
        finish.ClockDriftMs = model.DriftMs;
        finish.LastSyncedAtUtc = syncedAtUtc;

        await deviceRepo.UpdateAsync(starter, ct);
        await deviceRepo.UpdateAsync(finish, ct);

        session.SyncQualityScore = model.SyncQualityScore;
        if (model.SyncQualityScore >= ReadinessThreshold && session.Status != SessionStatus.Ready)
        {
            var devices = await deviceRepo.GetDevicesBySessionAsync(model.TimingSessionId, ct);
            var hasConnectedStarter = devices.Count(d =>
                d.DeviceRole == DeviceRole.Starter && d.Status == DeviceStatus.Connected) == 1;
            var hasConnectedFinish = devices.Any(d =>
                d.DeviceRole == DeviceRole.Finish && d.Status == DeviceStatus.Connected);
            var allRequiredConnected = devices
                .Where(d => d.DeviceRole is DeviceRole.Starter or DeviceRole.Finish)
                .All(d => d.Status == DeviceStatus.Connected);

            if (hasConnectedStarter && hasConnectedFinish && allRequiredConnected)
                session.Status = SessionStatus.Ready;
        }

        await sessionRepo.UpdateAsync(session, ct);
        await clockSyncRepo.CreateAsync(record, ct);
        return record;
    }

    public Task<ClockSyncRecord?> GetLatestSyncAsync(Guid sessionId, CancellationToken ct = default) =>
        clockSyncRepo.GetLatestSyncAsync(sessionId, ct);

    private static void ValidateDevice(TimingDevice device, Guid sessionId, DeviceRole requiredRole)
    {
        if (device.TimingSessionId != sessionId)
            throw new InvalidOperationException($"Device {device.Id} does not belong to session {sessionId}.");
        if (device.DeviceRole != requiredRole)
            throw new InvalidOperationException($"Device {device.Id} must have the {requiredRole} role.");
        if (device.Status != DeviceStatus.Connected)
            throw new InvalidOperationException($"Device {device.Id} must be connected.");
    }
}
