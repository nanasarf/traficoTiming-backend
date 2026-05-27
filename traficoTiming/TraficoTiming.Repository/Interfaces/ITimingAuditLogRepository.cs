using TraficoTiming.Database.Entities;

namespace TraficoTiming.Repository.Interfaces;

public interface ITimingAuditLogRepository
{
    Task<TimingAuditLog> CreateAsync(TimingAuditLog log, CancellationToken ct = default);
    Task<TimingAuditLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TimingAuditLog>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<TimingAuditLog>> GetByDeviceIdAsync(Guid deviceId, CancellationToken ct = default);
    Task<List<TimingAuditLog>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<TimingAuditLog>> GetRecentLogsAsync(int count, CancellationToken ct = default);
}
