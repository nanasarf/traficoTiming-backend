using Microsoft.EntityFrameworkCore;
using TraficoTiming.Database;
using TraficoTiming.Database.Entities;
using TraficoTiming.Repository.Interfaces;

namespace TraficoTiming.Repository.Implementations;

public class RaceStartEventRepository(TraficoTimingDbContext db) : IRaceStartEventRepository
{
    public async Task<RaceStartEvent> CreateAsync(RaceStartEvent raceStartEvent, CancellationToken ct = default)
    {
        db.RaceStartEvents.Add(raceStartEvent);
        await db.SaveChangesAsync(ct);
        return raceStartEvent;
    }

    public Task<RaceStartEvent?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.RaceStartEvents.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<List<RaceStartEvent>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.RaceStartEvents
            .Where(r => r.TimingSessionId == sessionId)
            .OrderByDescending(r => r.StartTimestampUtc)
            .ToListAsync(ct);

    public Task<RaceStartEvent?> GetLatestBySessionIdAsync(Guid sessionId, CancellationToken ct = default) =>
        db.RaceStartEvents
            .Where(r => r.TimingSessionId == sessionId)
            .OrderByDescending(r => r.StartTimestampUtc)
            .FirstOrDefaultAsync(ct);

    public Task<bool> ExistsForSessionAsync(Guid sessionId, CancellationToken ct = default) =>
        db.RaceStartEvents.AnyAsync(r => r.TimingSessionId == sessionId, ct);
}
