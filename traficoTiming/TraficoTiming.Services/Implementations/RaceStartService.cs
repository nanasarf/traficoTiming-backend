using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Implementations;

public class RaceStartService(
    IRaceStartEventRepository raceStartRepo,
    ITimingSessionRepository sessionRepo,
    ITimingDeviceRepository deviceRepo,
    IClockSyncRecordRepository clockSyncRepo,
    IAuditLogService auditLog) : IRaceStartService
{
    public async Task<RaceStartEvent> StartRaceAsync(StartRaceModel model, CancellationToken ct = default)
    {
        var session = await sessionRepo.GetByIdAsync(model.TimingSessionId, ct)
            ?? throw new KeyNotFoundException($"Session {model.TimingSessionId} not found.");

        if (session.Status != SessionStatus.Ready)
            throw new InvalidOperationException($"Race cannot start. Session status is '{session.Status}'. Status must be 'Ready'.");

        if (await raceStartRepo.ExistsForSessionAsync(model.TimingSessionId, ct))
            throw new InvalidOperationException("A race start event already exists for this session.");

        if (model.StartTimestampUtc == default || model.StartTimestampUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("StartTimestampUtc must be a valid UTC timestamp.", nameof(model.StartTimestampUtc));

        var starter = await deviceRepo.GetByIdAsync(model.StarterDeviceId, ct)
            ?? throw new KeyNotFoundException($"Device {model.StarterDeviceId} not found.");
        if (starter.TimingSessionId != model.TimingSessionId)
            throw new InvalidOperationException("The Starter device does not belong to this session.");
        if (starter.DeviceRole != DeviceRole.Starter)
            throw new InvalidOperationException("The selected device does not have the Starter role.");
        if (starter.Status != DeviceStatus.Connected)
            throw new InvalidOperationException("The Starter device must be connected.");

        var connectedFinishIds = (await deviceRepo.GetFinishDevicesAsync(model.TimingSessionId, ct))
            .Where(d => d.Status == DeviceStatus.Connected)
            .Select(d => d.Id)
            .ToArray();
        if (connectedFinishIds.Length == 0)
            throw new InvalidOperationException("At least one connected Finish device is required.");

        if (!await clockSyncRepo.ExistsAcceptedSyncAsync(
                model.TimingSessionId,
                model.StarterDeviceId,
                connectedFinishIds,
                ClockSyncService.ReadinessThreshold,
                ct))
            throw new InvalidOperationException("No accepted clock synchronization exists for the connected Starter and Finish devices.");

        var raceStart = new RaceStartEvent
        {
            TimingSessionId   = model.TimingSessionId,
            StarterDeviceId   = model.StarterDeviceId,
            StartTimestampUtc = model.StartTimestampUtc,
            StartMethod       = model.StartMethod,
            GunSoundPlayed    = model.GunSoundPlayed,
            FlashTriggered    = model.FlashTriggered
        };
        await raceStartRepo.CreateAsync(raceStart, ct);

        var previousStatus = session.Status;
        session.Status       = SessionStatus.Running;
        session.StartedAtUtc = raceStart.StartTimestampUtc;
        await sessionRepo.UpdateAsync(session, ct);
        await auditLog.LogAsync(session.Id, AuditActions.RaceStarted, raceStart.StarterDeviceId,
            details: new
            {
                previousStatus = previousStatus.ToString(),
                newStatus = session.Status.ToString(),
                reason = "race started",
                startTimestamp = raceStart.StartTimestampUtc,
                startMethod = raceStart.StartMethod.ToString()
            }, ct: ct);

        return raceStart;
    }

    public Task<RaceStartEvent?> GetRaceStartAsync(Guid sessionId, CancellationToken ct = default) =>
        raceStartRepo.GetLatestBySessionIdAsync(sessionId, ct);
}
