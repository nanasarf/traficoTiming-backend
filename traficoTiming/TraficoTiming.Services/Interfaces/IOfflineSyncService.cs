using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Interfaces;

public interface IOfflineSyncService
{
    Task<SyncBatch> ProcessSyncBatchAsync(SyncBatchModel model, CancellationToken ct = default);
    Task<SyncBatch?> GetSyncBatchAsync(Guid batchId, CancellationToken ct = default);
}
