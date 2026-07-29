namespace TraficoTiming.Api.Models.Requests;

public record UpdateRawTimingResultRequest(
    decimal? AdjustedTimeSeconds,
    string? ReviewerNote);
