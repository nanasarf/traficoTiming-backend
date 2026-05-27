using Microsoft.EntityFrameworkCore;
using TraficoTiming.Database;
using TraficoTiming.Database.Entities;
using TraficoTiming.Repository.Interfaces;

namespace TraficoTiming.Repository.Implementations;

public class TimingAuditLogRepository(TraficoTimingDbContext db) : ITimingAuditLogRepository
{
    public async Task<TimingAuditLog> CreateAsync(TimingAuditLog log, CancellationToken ct = default)
    {
        db.TimingAuditLogs.Add(log);
        await db.SaveChangesAsync(ct);
        return log;
    }

    public Task<TimingAuditLog?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.TimingAuditLogs.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<List<TimingAuditLog>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.TimingAuditLogs
            .Where(a => a.TimingSessionId == sessionId)
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<List<TimingAuditLog>> GetByDeviceIdAsync(Guid deviceId, CancellationToken ct = default) =>
        await db.TimingAuditLogs
            .Where(a => a.DeviceId == deviceId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<List<TimingAuditLog>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await db.TimingAuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<List<TimingAuditLog>> GetRecentLogsAsync(int count, CancellationToken ct = default) =>
        await db.TimingAuditLogs
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(count)
            .ToListAsync(ct);
}
