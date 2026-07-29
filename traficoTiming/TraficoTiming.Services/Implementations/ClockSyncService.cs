using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Implementations;

public class ClockSyncService(
    IClockSyncRecordRepository clockSyncRepo,
    ITimingSessionRepository sessionRepo,
    IAuditLogService auditLog) : IClockSyncService
{
    public async Task<ClockSyncRecord> SyncClocksAsync(ClockSyncModel model, CancellationToken ct = default)
    {
        if (model.SyncQualityScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(model.SyncQualityScore), "SyncQualityScore must be between 0 and 100.");

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
        await clockSyncRepo.CreateAsync(record, ct);

        // Promote session to Ready when sync quality is Fair or better (>= 70)
        if (model.SyncQualityScore >= 70)
        {
            var session = await sessionRepo.GetByIdAsync(model.TimingSessionId, ct);
            if (session?.Status == SessionStatus.DevicesPaired)
            {
                session.SyncQualityScore = model.SyncQualityScore;
                await sessionRepo.UpdateStatusAsync(model.TimingSessionId, SessionStatus.Ready, ct);
            }
        }

        return record;
    }

    public Task<ClockSyncRecord?> GetLatestSyncAsync(Guid sessionId, CancellationToken ct = default) =>
        clockSyncRepo.GetLatestSyncAsync(sessionId, ct);
}
