using TraficoTiming.Database.Entities;

namespace TraficoTiming.Repository.Interfaces;

public interface IClockSyncRecordRepository
{
    Task<ClockSyncRecord> CreateAsync(ClockSyncRecord record, CancellationToken ct = default);
    Task<ClockSyncRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ClockSyncRecord?> GetLatestSyncAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<ClockSyncRecord>> GetSessionSyncHistoryAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<ClockSyncRecord>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
}
