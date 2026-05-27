using TraficoTiming.Database.Entities;

namespace TraficoTiming.Repository.Interfaces;

public interface ISyncItemRepository
{
    Task<SyncItem> CreateAsync(SyncItem item, CancellationToken ct = default);
    Task<bool> ClientGeneratedIdExistsAsync(string clientGeneratedId, CancellationToken ct = default);
    Task MarkSyncItemCompletedAsync(Guid itemId, CancellationToken ct = default);
    Task MarkSyncItemFailedAsync(Guid itemId, string errorMessage, CancellationToken ct = default);
}
