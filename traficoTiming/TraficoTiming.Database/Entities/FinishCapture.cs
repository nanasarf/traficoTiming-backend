using TraficoTiming.Database.Enums;

namespace TraficoTiming.Database.Entities;

public class FinishCapture
{
    public Guid Id { get; set; }

    public Guid TimingSessionId { get; set; }
    public TimingSession TimingSession { get; set; } = null!;

    public Guid FinishDeviceId { get; set; }

    public string? VideoFileUrl { get; set; }
    public string? LocalFileId { get; set; }

    public decimal FrameRate { get; set; }
    public string Resolution { get; set; } = string.Empty;

    public DateTime RecordingStartedAtUtc { get; set; }
    public DateTime RecordingEndedAtUtc { get; set; }

    public string? FinishLineCalibrationDataJson { get; set; }

    public UploadStatus UploadStatus { get; set; } = UploadStatus.LocalOnly;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
