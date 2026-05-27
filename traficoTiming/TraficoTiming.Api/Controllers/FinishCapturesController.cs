using Microsoft.AspNetCore.Mvc;
using TraficoTiming.Api.Models.Requests;
using TraficoTiming.Api.Models.Responses;
using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Api.Controllers;

[ApiController]
[Route("api/timing")]
public class FinishCapturesController : ControllerBase
{
    private readonly IFinishCaptureService _finishCaptureService;

    public FinishCapturesController(IFinishCaptureService finishCaptureService)
    {
        _finishCaptureService = finishCaptureService;
    }

    [HttpPost("sessions/{sessionId:guid}/finish-captures")]
    public async Task<IActionResult> CreateCapture(
        Guid sessionId,
        [FromBody] CreateFinishCaptureRequest request,
        CancellationToken cancellationToken)
    {
        var model = new CreateFinishCaptureModel
        {
            TimingSessionId = sessionId,
            FinishDeviceId = request.FinishDeviceId,
            LocalFileId = request.LocalFileId,
            FrameRate = request.FrameRate,
            Resolution = request.Resolution,
            RecordingStartedAtUtc = request.RecordingStartedAtUtc,
            FinishLineCalibrationDataJson = request.FinishLineCalibrationDataJson
        };

        var capture = await _finishCaptureService.CreateCaptureAsync(model, cancellationToken);

        return Ok(ApiResponse<FinishCaptureResponse>.Ok(ToResponse(capture), "Finish capture created."));
    }

    [HttpGet("sessions/{sessionId:guid}/finish-captures")]
    public async Task<IActionResult> GetCaptures(Guid sessionId, CancellationToken cancellationToken)
    {
        var captures = await _finishCaptureService.GetCapturesAsync(sessionId, cancellationToken);

        var responses = captures.Select(ToResponse).ToList();

        return Ok(ApiResponse<List<FinishCaptureResponse>>.Ok(responses));
    }

    [HttpPatch("finish-captures/{captureId:guid}/upload-status")]
    public async Task<IActionResult> UpdateUploadStatus(
        Guid captureId,
        [FromBody] UpdateUploadStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _finishCaptureService.UpdateUploadStatusAsync(captureId, request.UploadStatus, cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Upload status updated."));
    }

    private static FinishCaptureResponse ToResponse(FinishCapture capture)
    {
        return new FinishCaptureResponse
        {
            Id = capture.Id,
            TimingSessionId = capture.TimingSessionId,
            FinishDeviceId = capture.FinishDeviceId,
            VideoFileUrl = capture.VideoFileUrl,
            LocalFileId = capture.LocalFileId,
            FrameRate = capture.FrameRate,
            Resolution = capture.Resolution,
            RecordingStartedAtUtc = capture.RecordingStartedAtUtc,
            RecordingEndedAtUtc = capture.RecordingEndedAtUtc,
            FinishLineCalibrationDataJson = capture.FinishLineCalibrationDataJson,
            UploadStatus = capture.UploadStatus,
            CreatedAtUtc = capture.CreatedAtUtc
        };
    }
}
