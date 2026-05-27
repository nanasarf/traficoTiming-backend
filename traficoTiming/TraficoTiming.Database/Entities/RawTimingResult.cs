using TraficoTiming.Database.Enums;

namespace TraficoTiming.Database.Entities;

public class RawTimingResult
{
    public Guid Id { get; set; }

    public Guid TimingSessionId { get; set; }
    public TimingSession TimingSession { get; set; } = null!;

    public Guid? AthleteId { get; set; }

    public int? Lane { get; set; }
    public string? BibNumber { get; set; }

    public DateTime DetectedFinishTimestampUtc { get; set; }

    public decimal RawTimeSeconds { get; set; }
    public decimal AdjustedTimeSeconds { get; set; }

    public DetectionMethod DetectionMethod { get; set; } = DetectionMethod.Manual;

    public decimal? ConfidenceScore { get; set; }

    public int? FrameNumber { get; set; }

    public RawResultStatus Status { get; set; } = RawResultStatus.Pending;

    public string? ReviewerNote { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
