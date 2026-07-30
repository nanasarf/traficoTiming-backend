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
        await auditLog.LogAsync(batch.TimingSessionId, AuditActions.SyncBatchReceived, batch.DeviceId,
            details: new { batchId = batch.Id, itemCount = batch.ItemCount }, ct: ct);

        try
        {
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
                    EntityType        = itemModel.EntityType,
                    PayloadJson       = itemModel.PayloadJson,
                    Status            = SyncStatus.Pending
                };
                await itemRepo.CreateAsync(item, ct);

                try
                {
                    await itemRepo.MarkSyncItemCompletedAsync(item.Id, ct);
                }
                catch (Exception)
                {
                    hasFailure = true;
                    await itemRepo.MarkSyncItemFailedAsync(item.Id, "Item processing failed.", ct);
                }
            }

            if (hasFailure)
            {
                await batchRepo.UpdateBatchStatusAsync(batch.Id, SyncStatus.Failed, "One or more items failed.", ct);
                await auditLog.LogAsync(batch.TimingSessionId, AuditActions.SyncBatchFailed, batch.DeviceId,
                    details: new
                    {
                        batchId = batch.Id,
                        itemCount = batch.ItemCount,
                        failureCategory = "item_processing"
                    }, ct: ct);
            }
            else
            {
                await batchRepo.MarkSyncBatchCompletedAsync(batch.Id, ct);
                await auditLog.LogAsync(batch.TimingSessionId, AuditActions.SyncBatchCompleted, batch.DeviceId,
                    details: new { batchId = batch.Id, itemCount = batch.ItemCount }, ct: ct);
            }

            return (await batchRepo.GetSyncBatchByIdAsync(batch.Id, ct))!;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await batchRepo.UpdateBatchStatusAsync(batch.Id, SyncStatus.Failed, "Batch processing failed.", ct);
            await auditLog.LogAsync(batch.TimingSessionId, AuditActions.SyncBatchFailed, batch.DeviceId,
                details: new
                {
                    batchId = batch.Id,
                    itemCount = batch.ItemCount,
                    failureCategory = GetFailureCategory(ex)
                }, ct: ct);
            throw;
        }
    }

    public Task<SyncBatch?> GetSyncBatchAsync(Guid batchId, CancellationToken ct = default) =>
        batchRepo.GetSyncBatchByIdAsync(batchId, ct);

    private static string GetFailureCategory(Exception exception) => exception switch
    {
        FormatException => "invalid_item_identifier",
        ArgumentException => "invalid_batch_item",
        _ => "batch_processing"
    };
}
