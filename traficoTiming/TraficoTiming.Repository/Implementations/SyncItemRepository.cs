using Microsoft.EntityFrameworkCore;
using TraficoTiming.Database;
using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;

namespace TraficoTiming.Repository.Implementations;

public class SyncItemRepository(TraficoTimingDbContext db) : ISyncItemRepository
{
    public async Task<SyncItem> CreateAsync(SyncItem item, CancellationToken ct = default)
    {
        db.SyncItems.Add(item);
        await db.SaveChangesAsync(ct);
        return item;
    }

    public Task<bool> ClientGeneratedIdExistsAsync(string clientGeneratedId, CancellationToken ct = default) =>
        db.SyncItems.AnyAsync(i => i.ClientGeneratedId == Guid.Parse(clientGeneratedId), ct);

    public async Task MarkSyncItemCompletedAsync(Guid itemId, CancellationToken ct = default) =>
        await db.SyncItems.Where(i => i.Id == itemId)
            .ExecuteUpdateAsync(i => i
                .SetProperty(x => x.Status, SyncStatus.Completed)
                .SetProperty(x => x.ProcessedAtUtc, DateTime.UtcNow), ct);

    public async Task MarkSyncItemFailedAsync(Guid itemId, string errorMessage, CancellationToken ct = default) =>
        await db.SyncItems.Where(i => i.Id == itemId)
            .ExecuteUpdateAsync(i => i
                .SetProperty(x => x.Status, SyncStatus.Failed)
                .SetProperty(x => x.ErrorMessage, errorMessage)
                .SetProperty(x => x.ProcessedAtUtc, DateTime.UtcNow), ct);
}
