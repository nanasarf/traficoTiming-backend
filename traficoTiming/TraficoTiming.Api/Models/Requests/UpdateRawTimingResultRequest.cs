namespace TraficoTiming.Api.Models.Requests;

public record UpdateRawTimingResultRequest(
    double? AdjustedTimeSeconds,
    string? ReviewerNote);
