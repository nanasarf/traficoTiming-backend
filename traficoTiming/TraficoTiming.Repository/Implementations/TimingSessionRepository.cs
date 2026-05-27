using Microsoft.EntityFrameworkCore;
using TraficoTiming.Database;
using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;

namespace TraficoTiming.Repository.Implementations;

public class TimingSessionRepository(TraficoTimingDbContext db) : ITimingSessionRepository
{
    public async Task<TimingSession> CreateAsync(TimingSession session, CancellationToken ct = default)
    {
        db.TimingSessions.Add(session);
        await db.SaveChangesAsync(ct);
        return session;
    }

    public Task<TimingSession?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.TimingSessions.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<TimingSession?> GetFullSessionAsync(Guid id, CancellationToken ct = default) =>
        db.TimingSessions
            .Include(s => s.Devices)
            .Include(s => s.ClockSyncRecords)
            .Include(s => s.RaceStartEvents)
            .Include(s => s.FinishCaptures)
            .Include(s => s.RawTimingResults)
            .Include(s => s.AuditLogs)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<List<TimingSession>> GetByMeetIdAsync(Guid meetId, CancellationToken ct = default) =>
        await db.TimingSessions
            .Where(s => s.MeetId == meetId)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<List<TimingSession>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default) =>
        await db.TimingSessions
            .Where(s => s.EventId == eventId)
            .OrderBy(s => s.HeatNumber)
            .ToListAsync(ct);

    public async Task<List<TimingSession>> GetRunningSessionsAsync(CancellationToken ct = default) =>
        await db.TimingSessions
            .Where(s => s.Status == SessionStatus.Running)
            .Include(s => s.Devices)
            .ToListAsync(ct);

    public async Task<List<TimingSession>> GetCompletedSessionsAsync(CancellationToken ct = default) =>
        await db.TimingSessions
            .Where(s => s.Status == SessionStatus.Completed)
            .OrderByDescending(s => s.EndedAtUtc)
            .ToListAsync(ct);

    public Task<TimingSession?> GetSessionWithDevicesAsync(Guid id, CancellationToken ct = default) =>
        db.TimingSessions
            .Include(s => s.Devices)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<TimingSession?> GetSessionWithResultsAsync(Guid id, CancellationToken ct = default) =>
        db.TimingSessions
            .Include(s => s.RawTimingResults)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task UpdateAsync(TimingSession session, CancellationToken ct = default)
    {
        db.TimingSessions.Update(session);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateStatusAsync(Guid sessionId, SessionStatus status, CancellationToken ct = default) =>
        await db.TimingSessions
            .Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, status), ct);

    public async Task DeleteAsync(TimingSession session, CancellationToken ct = default)
    {
        db.TimingSessions.Remove(session);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        db.TimingSessions.AnyAsync(s => s.Id == id, ct);
}
