using Microsoft.AspNetCore.Mvc;
using TraficoTiming.Api.Models.Requests;
using TraficoTiming.Api.Models.Responses;
using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Api.Controllers;

[ApiController]
[Route("api/timing")]
public class RaceStartController : ControllerBase
{
    private readonly IRaceStartService _raceStartService;

    public RaceStartController(IRaceStartService raceStartService)
    {
        _raceStartService = raceStartService;
    }

    [HttpPost("sessions/{sessionId:guid}/start")]
    public async Task<IActionResult> StartRace(
        Guid sessionId,
        [FromBody] StartRaceRequest request,
        CancellationToken cancellationToken)
    {
        var model = new StartRaceModel
        {
            TimingSessionId = sessionId,
            StarterDeviceId = request.StarterDeviceId,
            StartMethod = request.StartMethod,
            GunSoundPlayed = request.GunSoundPlayed,
            FlashTriggered = request.FlashTriggered
        };

        var raceStart = await _raceStartService.StartRaceAsync(model, cancellationToken);

        return Ok(ApiResponse<RaceStartResponse>.Ok(ToResponse(raceStart), "Race started."));
    }

    [HttpGet("sessions/{sessionId:guid}/start")]
    public async Task<IActionResult> GetRaceStart(Guid sessionId, CancellationToken cancellationToken)
    {
        var raceStart = await _raceStartService.GetRaceStartAsync(sessionId, cancellationToken);

        if (raceStart == null)
            return NotFound(ApiResponse<RaceStartResponse>.Fail("Race start not found."));

        return Ok(ApiResponse<RaceStartResponse>.Ok(ToResponse(raceStart)));
    }

    private static RaceStartResponse ToResponse(RaceStartEvent raceStart)
    {
        return new RaceStartResponse
        {
            Id = raceStart.Id,
            TimingSessionId = raceStart.TimingSessionId,
            StarterDeviceId = raceStart.StarterDeviceId,
            StartTimestampUtc = raceStart.StartTimestampUtc,
            StartMethod = raceStart.StartMethod,
            GunSoundPlayed = raceStart.GunSoundPlayed,
            FlashTriggered = raceStart.FlashTriggered,
            CreatedAtUtc = raceStart.CreatedAtUtc
        };
    }
}
