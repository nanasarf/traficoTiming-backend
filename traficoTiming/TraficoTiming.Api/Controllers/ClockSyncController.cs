using Microsoft.AspNetCore.Mvc;
using TraficoTiming.Api.Models.Requests;
using TraficoTiming.Api.Models.Responses;
using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Api.Controllers;

[ApiController]
[Route("api/timing")]
public class ClockSyncController : ControllerBase
{
    private readonly IClockSyncService _clockSyncService;

    public ClockSyncController(IClockSyncService clockSyncService)
    {
        _clockSyncService = clockSyncService;
    }

    [HttpPost("sessions/{sessionId:guid}/clock-sync")]
    public async Task<IActionResult> SyncClocks(
        Guid sessionId,
        [FromBody] ClockSyncRequest request,
        CancellationToken cancellationToken)
    {
        var model = new ClockSyncModel
        {
            TimingSessionId = sessionId,
            StarterDeviceId = request.StarterDeviceId,
            FinishDeviceId = request.FinishDeviceId,
            OffsetMs = request.OffsetMs,
            RoundTripDelayMs = request.RoundTripDelayMs,
            DriftMs = request.DriftMs,
            SyncQualityScore = request.SyncQualityScore
        };

        var sync = await _clockSyncService.SyncClocksAsync(model, cancellationToken);

        return Ok(ApiResponse<ClockSyncResponse>.Ok(ToResponse(sync), "Clock sync completed."));
    }

    [HttpGet("sessions/{sessionId:guid}/clock-sync/latest")]
    public async Task<IActionResult> GetLatestSync(Guid sessionId, CancellationToken cancellationToken)
    {
        var sync = await _clockSyncService.GetLatestSyncAsync(sessionId, cancellationToken);

        if (sync == null)
            return NotFound(ApiResponse<ClockSyncResponse>.Fail("No clock sync found for this session."));

        return Ok(ApiResponse<ClockSyncResponse>.Ok(ToResponse(sync)));
    }

    private static ClockSyncResponse ToResponse(ClockSyncRecord sync)
    {
        return new ClockSyncResponse
        {
            Id = sync.Id,
            TimingSessionId = sync.TimingSessionId,
            StarterDeviceId = sync.StarterDeviceId,
            FinishDeviceId = sync.FinishDeviceId,
            OffsetMs = sync.OffsetMs,
            RoundTripDelayMs = sync.RoundTripDelayMs,
            DriftMs = sync.DriftMs,
            SyncQualityScore = sync.SyncQualityScore,
            CreatedAtUtc = sync.CreatedAtUtc
        };
    }
}
