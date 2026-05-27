using TraficoTiming.Database.Enums;

namespace TraficoTiming.Api.Models.Responses;

public class RawTimingResultResponse
{
    public Guid Id { get; set; }
    public Guid TimingSessionId { get; set; }
    public Guid? AthleteId { get; set; }
    public int? Lane { get; set; }
    public string? BibNumber { get; set; }
    public DateTime DetectedFinishTimestampUtc { get; set; }
    public decimal RawTimeSeconds { get; set; }
    public decimal AdjustedTimeSeconds { get; set; }
    public DetectionMethod DetectionMethod { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public int? FrameNumber { get; set; }
    public RawResultStatus Status { get; set; }
    public string? ReviewerNote { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
