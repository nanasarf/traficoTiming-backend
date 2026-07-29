namespace TraficoTiming.Services.Models;

public class UpdateRawTimingResultModel
{
    public Guid ResultId { get; set; }
    public decimal? AdjustedTimeSeconds { get; set; }
    public string? ReviewerNote { get; set; }
}
