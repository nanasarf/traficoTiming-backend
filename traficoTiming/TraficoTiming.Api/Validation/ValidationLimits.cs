namespace TraficoTiming.Api.Validation;

public static class ValidationLimits
{
    public const int RoundLength = 50;
    public const int DeviceNameLength = 100;
    public const int DeviceTypeLength = 50;
    public const int ResolutionLength = 50;
    public const int LocalFileIdLength = 200;
    public const int VideoFileUrlLength = 1000;
    public const int BibNumberLength = 50;
    public const int ReviewerNoteLength = 1000;
    public const int EntityTypeLength = 100;
    public const int PayloadJsonLength = 1_048_576;

    // Broad transport sanity bounds. Values outside these indicate bad units or corrupt input.
    public const decimal MaximumClockOffsetMs = 60_000;
    public const decimal MaximumRoundTripDelayMs = 60_000;
    public const decimal MaximumDriftMs = 10_000;
    public const decimal MaximumFrameRate = 1_000;
}
