using TraficoTiming.Database.Entities;

namespace TraficoTiming.Services.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(Guid sessionId, string action, Guid? deviceId = null, Guid? userId = null, object? details = null, CancellationToken ct = default);
    Task<IReadOnlyList<TimingAuditLog>> GetLogsAsync(Guid sessionId, CancellationToken ct = default);
}
