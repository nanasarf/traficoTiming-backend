using Microsoft.EntityFrameworkCore;
using TraficoTiming.Database;
using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;

namespace TraficoTiming.Repository.Implementations;

public class RawTimingResultRepository(TraficoTimingDbContext db) : IRawTimingResultRepository
{
    public async Task<RawTimingResult> CreateRawResultAsync(RawTimingResult result, CancellationToken ct = default)
    {
        db.RawTimingResults.Add(result);
        await db.SaveChangesAsync(ct);
        return result;
    }

    public Task<RawTimingResult?> GetRawResultByIdAsync(Guid id, CancellationToken ct = default) =>
        db.RawTimingResults.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<List<RawTimingResult>> GetResultsBySessionAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.RawTimingResults
            .Where(r => r.TimingSessionId == sessionId)
            .OrderBy(r => r.Lane)
            .ThenBy(r => r.RawTimeSeconds)
            .ToListAsync(ct);

    public async Task<List<RawTimingResult>> GetResultsByLaneAsync(Guid sessionId, int lane, CancellationToken ct = default) =>
        await db.RawTimingResults
            .Where(r => r.TimingSessionId == sessionId && r.Lane == lane)
            .OrderBy(r => r.RawTimeSeconds)
            .ToListAsync(ct);

    public async Task<List<RawTimingResult>> GetPendingResultsAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.RawTimingResults
            .Where(r => r.TimingSessionId == sessionId && r.Status == RawResultStatus.Pending)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<List<RawTimingResult>> GetSubmittedResultsAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.RawTimingResults
            .Where(r => r.TimingSessionId == sessionId && r.Status == RawResultStatus.Submitted)
            .OrderBy(r => r.Lane)
            .ThenBy(r => r.AdjustedTimeSeconds)
            .ToListAsync(ct);

    public async Task<List<RawTimingResult>> GetResultsByStatusAsync(Guid sessionId, RawResultStatus status, CancellationToken ct = default) =>
        await db.RawTimingResults
            .Where(r => r.TimingSessionId == sessionId && r.Status == status)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task UpdateRawResultAsync(RawTimingResult result, CancellationToken ct = default)
    {
        result.UpdatedAtUtc = DateTime.UtcNow;
        db.RawTimingResults.Update(result);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(RawTimingResult result, CancellationToken ct = default)
    {
        db.RawTimingResults.Remove(result);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> LaneExistsInSessionAsync(Guid sessionId, int lane, CancellationToken ct = default) =>
        db.RawTimingResults.AnyAsync(r => r.TimingSessionId == sessionId && r.Lane == lane, ct);
}
