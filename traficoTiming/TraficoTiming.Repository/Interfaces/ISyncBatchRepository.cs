using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;

namespace TraficoTiming.Repository.Interfaces;

public interface ISyncBatchRepository
{
    Task<SyncBatch> CreateSyncBatchAsync(SyncBatch batch, CancellationToken ct = default);
    Task<SyncBatch?> GetSyncBatchByIdAsync(Guid batchId, CancellationToken ct = default);
    Task UpdateBatchStatusAsync(Guid batchId, SyncStatus status, string? errorMessage = null, CancellationToken ct = default);
    Task MarkSyncBatchCompletedAsync(Guid batchId, CancellationToken ct = default);
}
