using TraficoTiming.Database.Enums;

namespace TraficoTiming.Services.Models;

public class CreateRawTimingResultModel
{
    public Guid TimingSessionId { get; set; }
    public Guid? AthleteId { get; set; }
    public int? Lane { get; set; }
    public string? BibNumber { get; set; }
    public DateTime? DetectedFinishTimestampUtc { get; set; }
    public decimal RawTimeSeconds { get; set; }
    public decimal? AdjustedTimeSeconds { get; set; }
    public DetectionMethod DetectionMethod { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public long? FrameNumber { get; set; }
}
