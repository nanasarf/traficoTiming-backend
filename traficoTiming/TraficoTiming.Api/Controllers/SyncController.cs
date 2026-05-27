using Microsoft.AspNetCore.Mvc;
using TraficoTiming.Api.Models.Requests;
using TraficoTiming.Api.Models.Responses;
using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Api.Controllers;

[ApiController]
[Route("api/timing/sync")]
public class SyncController : ControllerBase
{
    private readonly IOfflineSyncService _offlineSyncService;

    public SyncController(IOfflineSyncService offlineSyncService)
    {
        _offlineSyncService = offlineSyncService;
    }

    [HttpPost("batch")]
    public async Task<IActionResult> ProcessBatch(
        [FromBody] SyncBatchRequest request,
        CancellationToken cancellationToken)
    {
        var model = new SyncBatchModel
        {
            TimingSessionId = request.TimingSessionId,
            DeviceId = request.DeviceId,
            Items = request.Items.Select(i => new SyncItemModel
            {
                ClientGeneratedId = i.ClientGeneratedId.ToString(),
                EntityType = i.EntityType,
                PayloadJson = i.PayloadJson
            }).ToList()
        };

        var batch = await _offlineSyncService.ProcessSyncBatchAsync(model, cancellationToken);

        return Ok(ApiResponse<SyncBatchResponse>.Ok(ToResponse(batch), "Sync batch processed."));
    }

    [HttpGet("batch/{batchId:guid}")]
    public async Task<IActionResult> GetBatch(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await _offlineSyncService.GetSyncBatchAsync(batchId, cancellationToken);

        if (batch == null)
            return NotFound(ApiResponse<SyncBatchResponse>.Fail("Sync batch not found."));

        return Ok(ApiResponse<SyncBatchResponse>.Ok(ToResponse(batch)));
    }

    private static SyncBatchResponse ToResponse(SyncBatch batch)
    {
        return new SyncBatchResponse
        {
            Id = batch.Id,
            TimingSessionId = batch.TimingSessionId,
            DeviceId = batch.DeviceId,
            Status = batch.Status,
            ItemCount = batch.ItemCount,
            CreatedAtUtc = batch.CreatedAtUtc,
            ProcessedAtUtc = batch.ProcessedAtUtc,
            ErrorMessage = batch.ErrorMessage
        };
    }
}
