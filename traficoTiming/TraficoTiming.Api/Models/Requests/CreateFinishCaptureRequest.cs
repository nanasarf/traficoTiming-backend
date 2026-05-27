namespace TraficoTiming.Api.Models.Requests;

public record CreateFinishCaptureRequest(
    Guid FinishDeviceId,
    string? LocalFileId,
    double? FrameRate,
    string? Resolution,
    DateTime? RecordingStartedAtUtc,
    string? FinishLineCalibrationDataJson);
