using TraficoTiming.Database.Enums;

public record UpdateRawTimingResultRequest(
    decimal? AdjustedTimeSeconds,
    string? ReviewerNote,
    decimal? RawTimeSeconds = null,
    DetectionMethod? DetectionMethod = null,
    decimal? ConfidenceScore = null,
    int? Lane = null,
    string? BibNumber = null,
    DateTime? DetectedFinishTimestampUtc = null,
    long? FrameNumber = null);
