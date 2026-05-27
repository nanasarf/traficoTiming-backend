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
        var qualityScore = CalculateQualityScore(model.RoundTripDelayMs, model.DriftMs);

        var record = new ClockSyncRecord
        {
            TimingSessionId  = model.TimingSessionId,
            StarterDeviceId  = model.StarterDeviceId,
            FinishDeviceId   = model.FinishDeviceId,
            OffsetMs         = (decimal)model.OffsetMs,
            RoundTripDelayMs = (decimal)model.RoundTripDelayMs,
            DriftMs          = (decimal?)model.DriftMs,
            SyncQualityScore = (decimal)qualityScore
        };
        await clockSyncRepo.CreateAsync(record, ct);

        // Promote session to Ready when sync quality is Fair or better (>= 70)
        if (qualityScore >= 70)
        {
            var session = await sessionRepo.GetByIdAsync(model.TimingSessionId, ct);
            if (session?.Status == SessionStatus.DevicesPaired)
            {
                session.SyncQualityScore = (decimal?)qualityScore;
                await sessionRepo.UpdateStatusAsync(model.TimingSessionId, SessionStatus.Ready, ct);
            }
        }

        return record;
    }

    public Task<ClockSyncRecord?> GetLatestSyncAsync(Guid sessionId, CancellationToken ct = default) =>
        clockSyncRepo.GetLatestSyncAsync(sessionId, ct);

    private static double CalculateQualityScore(long roundTripDelayMs, long driftMs)
    {
        double delayPenalty = Math.Min(roundTripDelayMs / 2.0, 50);
        double driftPenalty = Math.Min(Math.Abs(driftMs) / 2.0, 50);
        return Math.Max(0, 100 - delayPenalty - driftPenalty);
    }
}
