using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Implementations;

public class OfflineSyncService(
    ISyncBatchRepository batchRepo,
    ISyncItemRepository itemRepo,
    IAuditLogService auditLog) : IOfflineSyncService
{
    public async Task<SyncBatch> ProcessSyncBatchAsync(SyncBatchModel model, CancellationToken ct = default)
    {
        var batch = new SyncBatch
        {
            TimingSessionId = model.TimingSessionId,
            DeviceId        = model.DeviceId,
            Status          = SyncStatus.Processing,
            ItemCount       = model.Items.Count
        };
        await batchRepo.CreateSyncBatchAsync(batch, ct);

        var hasFailure = false;
        foreach (var itemModel in model.Items)
        {
            // Idempotency: skip items already seen
            if (await itemRepo.ClientGeneratedIdExistsAsync(itemModel.ClientGeneratedId, ct))
                continue;

            var item = new SyncItem
            {
                SyncBatchId       = batch.Id,
                ClientGeneratedId = Guid.Parse(itemModel.ClientGeneratedId),
                EntityType        = itemModel.EntityType.ToString(),
                PayloadJson       = itemModel.PayloadJson,
                Status            = SyncStatus.Pending
            };
            await itemRepo.CreateAsync(item, ct);

            try
            {
                await itemRepo.MarkSyncItemCompletedAsync(item.Id, ct);
            }
            catch (Exception ex)
            {
                hasFailure = true;
                await itemRepo.MarkSyncItemFailedAsync(item.Id, ex.Message, ct);
            }
        }

        if (hasFailure)
        {
            await batchRepo.UpdateBatchStatusAsync(batch.Id, SyncStatus.Failed, "One or more items failed.", ct);
        }
        else
        {
            await batchRepo.MarkSyncBatchCompletedAsync(batch.Id, ct);
        }

        return (await batchRepo.GetSyncBatchByIdAsync(batch.Id, ct))!;
    }

    public Task<SyncBatch?> GetSyncBatchAsync(Guid batchId, CancellationToken ct = default) =>
        batchRepo.GetSyncBatchByIdAsync(batchId, ct);
}
