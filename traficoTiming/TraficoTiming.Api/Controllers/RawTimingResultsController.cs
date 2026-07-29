using Microsoft.AspNetCore.Mvc;
using TraficoTiming.Api.Models.Requests;
using TraficoTiming.Api.Models.Responses;
using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Api.Controllers;

[ApiController]
[Route("api/timing")]
public class RawTimingResultsController : ControllerBase
{
    private readonly IRawTimingResultService _rawTimingResultService;

    public RawTimingResultsController(IRawTimingResultService rawTimingResultService)
    {
        _rawTimingResultService = rawTimingResultService;
    }

    [HttpPost("sessions/{sessionId:guid}/raw-results")]
    public async Task<IActionResult> CreateResult(
        Guid sessionId,
        [FromBody] CreateRawTimingResultRequest request,
        CancellationToken cancellationToken)
    {
        var model = new CreateRawTimingResultModel
        {
            TimingSessionId = sessionId,
            AthleteId = request.AthleteId,
            Lane = request.Lane,
            BibNumber = request.BibNumber,
            DetectedFinishTimestampUtc = request.DetectedFinishTimestampUtc,
            RawTimeSeconds = request.RawTimeSeconds,
            AdjustedTimeSeconds = request.AdjustedTimeSeconds,
            DetectionMethod = request.DetectionMethod,
            ConfidenceScore = request.ConfidenceScore,
            FrameNumber = request.FrameNumber
        };

        var result = await _rawTimingResultService.CreateResultAsync(model, cancellationToken);

        return Ok(ApiResponse<RawTimingResultResponse>.Ok(ToResponse(result), "Raw timing result created."));
    }

    [HttpGet("sessions/{sessionId:guid}/raw-results")]
    public async Task<IActionResult> GetSessionResults(Guid sessionId, CancellationToken cancellationToken)
    {
        var results = await _rawTimingResultService.GetSessionResultsAsync(sessionId, cancellationToken);

        var responses = results.Select(ToResponse).ToList();

        return Ok(ApiResponse<List<RawTimingResultResponse>>.Ok(responses));
    }

    [HttpGet("raw-results/{resultId:guid}")]
    public async Task<IActionResult> GetResult(Guid resultId, CancellationToken cancellationToken)
    {
        var result = await _rawTimingResultService.GetResultAsync(resultId, cancellationToken);

        if (result == null)
            return NotFound(ApiResponse<RawTimingResultResponse>.Fail("Raw timing result not found."));

        return Ok(ApiResponse<RawTimingResultResponse>.Ok(ToResponse(result)));
    }

    [HttpPatch("raw-results/{resultId:guid}")]
    public async Task<IActionResult> UpdateResult(
        Guid resultId,
        [FromBody] UpdateRawTimingResultRequest request,
        CancellationToken cancellationToken)
    {
        var model = new UpdateRawTimingResultModel
        {
            ResultId = resultId,
            AdjustedTimeSeconds = request.AdjustedTimeSeconds,
            ReviewerNote = request.ReviewerNote
        };

        var result = await _rawTimingResultService.UpdateResultAsync(model, cancellationToken);

        return Ok(ApiResponse<RawTimingResultResponse>.Ok(ToResponse(result), "Raw timing result updated."));
    }

    [HttpPost("raw-results/{resultId:guid}/submit")]
    public async Task<IActionResult> SubmitResult(Guid resultId, CancellationToken cancellationToken)
    {
        var result = await _rawTimingResultService.SubmitResultAsync(resultId, cancellationToken);

        return Ok(ApiResponse<RawTimingResultResponse>.Ok(ToResponse(result), "Raw timing result submitted."));
    }

    [HttpPost("raw-results/{resultId:guid}/reject")]
    public async Task<IActionResult> RejectResult(
        Guid resultId,
        [FromBody] RejectRawTimingResultRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _rawTimingResultService.RejectResultAsync(resultId, request.ReviewerNote ?? string.Empty, cancellationToken);

        return Ok(ApiResponse<RawTimingResultResponse>.Ok(ToResponse(result), "Raw timing result rejected."));
    }

    private static RawTimingResultResponse ToResponse(RawTimingResult result)
    {
        return new RawTimingResultResponse
        {
            Id = result.Id,
            TimingSessionId = result.TimingSessionId,
            AthleteId = result.AthleteId,
            Lane = result.Lane,
            BibNumber = result.BibNumber,
            DetectedFinishTimestampUtc = result.DetectedFinishTimestampUtc,
            RawTimeSeconds = result.RawTimeSeconds,
            AdjustedTimeSeconds = result.AdjustedTimeSeconds,
            DetectionMethod = result.DetectionMethod,
            ConfidenceScore = result.ConfidenceScore,
            FrameNumber = result.FrameNumber,
            Status = result.Status,
            ReviewerNote = result.ReviewerNote,
            CreatedAtUtc = result.CreatedAtUtc,
            UpdatedAtUtc = result.UpdatedAtUtc
        };
    }
}
