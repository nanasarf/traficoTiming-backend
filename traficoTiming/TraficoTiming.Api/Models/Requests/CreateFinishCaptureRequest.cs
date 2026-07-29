namespace TraficoTiming.Api.Models.Requests;

public record CreateFinishCaptureRequest(
    Guid FinishDeviceId,
    string? LocalFileId,
    decimal? FrameRate,
    string? Resolution,
    DateTime? RecordingStartedAtUtc,
    DateTime RecordingEndedAtUtc,
    string? FinishLineCalibrationDataJson);
