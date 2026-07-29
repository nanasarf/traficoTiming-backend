namespace TraficoTiming.Api.Validation;

public sealed class RequestValidationOptions
{
    public const string SectionName = "RequestValidation";

    public int MaximumLane { get; set; } = 20;
    public int MaximumSyncBatchItems { get; set; } = 500;
}
