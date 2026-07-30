using Microsoft.EntityFrameworkCore;
using TraficoTiming.Database;
using TraficoTiming.Database.Entities;
using TraficoTiming.Repository.Interfaces;

namespace TraficoTiming.Repository.Implementations;

public class ClockSyncRecordRepository(TraficoTimingDbContext db) : IClockSyncRecordRepository
{
    public async Task<ClockSyncRecord> CreateAsync(ClockSyncRecord record, CancellationToken ct = default)
    {
        db.ClockSyncRecords.Add(record);
        await db.SaveChangesAsync(ct);
        return record;
    }

    public Task<ClockSyncRecord?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.ClockSyncRecords.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<ClockSyncRecord?> GetLatestSyncAsync(Guid sessionId, CancellationToken ct = default) =>
        db.ClockSyncRecords
            .Where(c => c.TimingSessionId == sessionId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    public Task<bool> ExistsAcceptedSyncAsync(
        Guid sessionId,
        Guid starterDeviceId,
        IReadOnlyCollection<Guid> finishDeviceIds,
        decimal minimumQualityScore,
        CancellationToken ct = default) =>
        db.ClockSyncRecords.AnyAsync(
            c => c.TimingSessionId == sessionId
                && c.StarterDeviceId == starterDeviceId
                && finishDeviceIds.Contains(c.FinishDeviceId)
                && c.SyncQualityScore >= minimumQualityScore,
            ct);

    public async Task<List<ClockSyncRecord>> GetSessionSyncHistoryAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.ClockSyncRecords
            .Where(c => c.TimingSessionId == sessionId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<List<ClockSyncRecord>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.ClockSyncRecords
            .Where(c => c.TimingSessionId == sessionId)
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync(ct);
}
