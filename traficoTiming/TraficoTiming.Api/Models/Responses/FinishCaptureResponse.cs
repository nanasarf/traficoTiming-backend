using TraficoTiming.Database.Enums;

namespace TraficoTiming.Api.Models.Responses;

public class FinishCaptureResponse
{
    public Guid Id { get; set; }
    public Guid TimingSessionId { get; set; }
    public Guid FinishDeviceId { get; set; }
    public string? VideoFileUrl { get; set; }
    public string? LocalFileId { get; set; }
    public decimal FrameRate { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public DateTime RecordingStartedAtUtc { get; set; }
    public DateTime RecordingEndedAtUtc { get; set; }
    public string? FinishLineCalibrationDataJson { get; set; }
    public UploadStatus UploadStatus { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
