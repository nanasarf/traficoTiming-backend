using Microsoft.EntityFrameworkCore;
using TraficoTiming.Database;
using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;

namespace TraficoTiming.Repository.Implementations;

public class SyncBatchRepository(TraficoTimingDbContext db) : ISyncBatchRepository
{
    public async Task<SyncBatch> CreateSyncBatchAsync(SyncBatch batch, CancellationToken ct = default)
    {
        db.SyncBatches.Add(batch);
        await db.SaveChangesAsync(ct);
        return batch;
    }

    public Task<SyncBatch?> GetSyncBatchByIdAsync(Guid batchId, CancellationToken ct = default) =>
        db.SyncBatches.Include(b => b.SyncItems)
            .FirstOrDefaultAsync(b => b.Id == batchId, ct);

    public async Task UpdateBatchStatusAsync(Guid batchId, SyncStatus status, string? errorMessage = null, CancellationToken ct = default) =>
        await db.SyncBatches.Where(b => b.Id == batchId)
            .ExecuteUpdateAsync(b => b
                .SetProperty(x => x.Status, status)
                .SetProperty(x => x.ErrorMessage, errorMessage), ct);

    public async Task MarkSyncBatchCompletedAsync(Guid batchId, CancellationToken ct = default) =>
        await db.SyncBatches.Where(b => b.Id == batchId)
            .ExecuteUpdateAsync(b => b
                .SetProperty(x => x.Status, SyncStatus.Completed)
                .SetProperty(x => x.ProcessedAtUtc, DateTime.UtcNow), ct);
}
