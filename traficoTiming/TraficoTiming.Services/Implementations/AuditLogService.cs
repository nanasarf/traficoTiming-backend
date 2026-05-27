using System.Text.Json;
using TraficoTiming.Database.Entities;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Interfaces;

namespace TraficoTiming.Services.Implementations;

public class AuditLogService(ITimingAuditLogRepository auditRepo) : IAuditLogService
{
    public async Task LogAsync(Guid sessionId, string action, Guid? deviceId = null, Guid? userId = null, object? details = null, CancellationToken ct = default)
    {
        var log = new TimingAuditLog
        {
            TimingSessionId = sessionId,
            Action          = action,
            DeviceId        = deviceId,
            UserId          = userId,
            DetailsJson     = details is not null ? JsonSerializer.Serialize(details) : null
        };
        await auditRepo.CreateAsync(log, ct);
    }

    public async Task<IReadOnlyList<TimingAuditLog>> GetLogsAsync(Guid sessionId, CancellationToken ct = default) =>
        (await auditRepo.GetBySessionIdAsync(sessionId, ct)).AsReadOnly();
}
