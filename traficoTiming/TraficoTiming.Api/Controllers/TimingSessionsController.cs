using Microsoft.AspNetCore.Mvc;
using TraficoTiming.Api.Models.Requests;
using TraficoTiming.Api.Models.Responses;
using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Api.Controllers;

[ApiController]
[Route("api/timing/sessions")]
public class TimingSessionsController : ControllerBase
{
    private readonly ITimingSessionService _timingSessionService;

    public TimingSessionsController(ITimingSessionService timingSessionService)
    {
        _timingSessionService = timingSessionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreateTimingSessionRequest request,
        CancellationToken cancellationToken)
    {
        var model = new CreateTimingSessionModel
        {
            MeetId = request.MeetId,
            EventId = request.EventId,
            HeatNumber = request.HeatNumber,
            Round = request.Round,
            CreatedByUserId = request.CreatedByUserId
        };

        var session = await _timingSessionService.CreateSessionAsync(model, cancellationToken);

        return Ok(ApiResponse<TimingSessionResponse>.Ok(ToResponse(session), "Timing session created."));
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<IActionResult> GetSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _timingSessionService.GetSessionAsync(sessionId, cancellationToken);

        if (session == null)
            return NotFound(ApiResponse<TimingSessionResponse>.Fail("Timing session not found."));

        return Ok(ApiResponse<TimingSessionResponse>.Ok(ToResponse(session)));
    }

    [HttpGet("meet/{meetId:guid}")]
    public async Task<IActionResult> GetSessionsByMeet(Guid meetId, CancellationToken cancellationToken)
    {
        var sessions = await _timingSessionService.GetSessionsByMeetAsync(meetId, cancellationToken);

        var responses = sessions.Select(ToResponse).ToList();

        return Ok(ApiResponse<List<TimingSessionResponse>>.Ok(responses));
    }

    [HttpPatch("{sessionId:guid}/end")]
    public async Task<IActionResult> EndSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _timingSessionService.EndSessionAsync(sessionId, cancellationToken);

        return Ok(ApiResponse<TimingSessionResponse>.Ok(ToResponse(session), "Timing session ended."));
    }

    [HttpPatch("{sessionId:guid}/cancel")]
    public async Task<IActionResult> CancelSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _timingSessionService.CancelSessionAsync(sessionId, cancellationToken);

        return Ok(ApiResponse<TimingSessionResponse>.Ok(ToResponse(session), "Timing session cancelled."));
    }

    private static TimingSessionResponse ToResponse(TimingSession session)
    {
        return new TimingSessionResponse
        {
            Id = session.Id,
            MeetId = session.MeetId,
            EventId = session.EventId,
            HeatNumber = session.HeatNumber,
            Round = session.Round,
            Status = session.Status,
            CreatedByUserId = session.CreatedByUserId,
            CreatedAtUtc = session.CreatedAtUtc,
            StartedAtUtc = session.StartedAtUtc,
            EndedAtUtc = session.EndedAtUtc,
            SyncQualityScore = session.SyncQualityScore
        };
    }
}
