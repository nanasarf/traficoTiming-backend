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

        if (!await deviceRepo.RoleExistsInSessionAsync(model.TimingSessionId, DeviceRole.Starter, ct))
            throw new InvalidOperationException("No Starter device is paired to this session.");

        if (!await deviceRepo.RoleExistsInSessionAsync(model.TimingSessionId, DeviceRole.Finish, ct))
            throw new InvalidOperationException("No Finish device is paired to this session.");

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

        session.Status       = SessionStatus.Running;
        session.StartedAtUtc = raceStart.StartTimestampUtc;
        await sessionRepo.UpdateAsync(session, ct);

        return raceStart;
    }

    public Task<RaceStartEvent?> GetRaceStartAsync(Guid sessionId, CancellationToken ct = default) =>
        raceStartRepo.GetLatestBySessionIdAsync(sessionId, ct);
}
