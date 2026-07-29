namespace TraficoTiming.Services.Models;

public class CreateFinishCaptureModel
{
    public Guid TimingSessionId { get; set; }
    public Guid FinishDeviceId { get; set; }
    public string? LocalFileId { get; set; }
    public decimal? FrameRate { get; set; }
    public string? Resolution { get; set; }
    public DateTime? RecordingStartedAtUtc { get; set; }
    public DateTime RecordingEndedAtUtc { get; set; }
    public string? FinishLineCalibrationDataJson { get; set; }
}
