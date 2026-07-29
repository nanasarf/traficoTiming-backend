using FluentValidation;
using Microsoft.Extensions.Options;
using TraficoTiming.Api.Models.Requests;

namespace TraficoTiming.Api.Validation;

public sealed class CreateTimingSessionRequestValidator : AbstractValidator<CreateTimingSessionRequest>
{
    public CreateTimingSessionRequestValidator()
    {
        RuleFor(x => x.MeetId).NotEmpty();
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.HeatNumber).GreaterThan(0);
        RuleFor(x => x.Round).MaximumLength(ValidationLimits.RoundLength);
    }
}

public sealed class PairDeviceRequestValidator : AbstractValidator<PairDeviceRequest>
{
    public PairDeviceRequestValidator()
    {
        RuleFor(x => x.DeviceName).NotEmpty().MaximumLength(ValidationLimits.DeviceNameLength);
        RuleFor(x => x.DeviceType).NotEmpty().MaximumLength(ValidationLimits.DeviceTypeLength);
        RuleFor(x => x.DeviceRole).IsInEnum();
        RuleFor(x => x.ConnectionType).IsInEnum();
        RuleFor(x => x.BatteryLevel).InclusiveBetween(0, 100).When(x => x.BatteryLevel.HasValue);
    }
}

public sealed class ClockSyncRequestValidator : AbstractValidator<ClockSyncRequest>
{
    public ClockSyncRequestValidator()
    {
        RuleFor(x => x.StarterDeviceId).NotEmpty();
        RuleFor(x => x.FinishDeviceId).NotEmpty()
            .NotEqual(x => x.StarterDeviceId).WithMessage("FinishDeviceId must be different from StarterDeviceId.");
        RuleFor(x => x.OffsetMs)
            .InclusiveBetween(-ValidationLimits.MaximumClockOffsetMs, ValidationLimits.MaximumClockOffsetMs);
        RuleFor(x => x.RoundTripDelayMs)
            .InclusiveBetween(0, ValidationLimits.MaximumRoundTripDelayMs);
        RuleFor(x => x.SyncQualityScore).InclusiveBetween(0, 100);
        RuleFor(x => x.DriftMs)
            .InclusiveBetween(-ValidationLimits.MaximumDriftMs, ValidationLimits.MaximumDriftMs)
            .When(x => x.DriftMs.HasValue);
    }
}

public sealed class StartRaceRequestValidator : AbstractValidator<StartRaceRequest>
{
    public StartRaceRequestValidator()
    {
        RuleFor(x => x.StarterDeviceId).NotEmpty();
        RuleFor(x => x.StartTimestampUtc).NotEmpty()
            .Must(value => value.Kind == DateTimeKind.Utc).WithMessage("StartTimestampUtc must be UTC.");
        RuleFor(x => x.StartMethod).IsInEnum();
    }
}

public sealed class CreateFinishCaptureRequestValidator : AbstractValidator<CreateFinishCaptureRequest>
{
    public CreateFinishCaptureRequestValidator()
    {
        RuleFor(x => x.FinishDeviceId).NotEmpty();
        RuleFor(x => x.FrameRate).NotNull()
            .InclusiveBetween(0.001m, ValidationLimits.MaximumFrameRate);
        RuleFor(x => x.Resolution).NotEmpty().MaximumLength(ValidationLimits.ResolutionLength);
        RuleFor(x => x.RecordingStartedAtUtc).NotNull();
        RuleFor(x => x.RecordingEndedAtUtc).NotEmpty()
            .GreaterThan(x => x.RecordingStartedAtUtc!.Value)
            .When(x => x.RecordingStartedAtUtc.HasValue);
        RuleFor(x => x.FinishLineCalibrationDataJson)
            .Must(JsonValidation.IsValid).WithMessage("FinishLineCalibrationDataJson must contain valid JSON.")
            .When(x => x.FinishLineCalibrationDataJson is not null);
        RuleFor(x => x.LocalFileId).MaximumLength(ValidationLimits.LocalFileIdLength);
        RuleFor(x => x.VideoFileUrl).MaximumLength(ValidationLimits.VideoFileUrlLength);
    }
}

public sealed class CreateRawTimingResultRequestValidator : AbstractValidator<CreateRawTimingResultRequest>
{
    public CreateRawTimingResultRequestValidator(IOptions<RequestValidationOptions> options)
    {
        var maximumLane = options.Value.MaximumLane;

        RuleFor(x => x).Must(x => x.AthleteId.HasValue || !string.IsNullOrWhiteSpace(x.BibNumber) || x.Lane.HasValue)
            .WithMessage("At least one of AthleteId, BibNumber, or Lane must identify the result.");
        RuleFor(x => x.AthleteId).NotEqual(Guid.Empty).When(x => x.AthleteId.HasValue);
        RuleFor(x => x.Lane).InclusiveBetween(1, maximumLane).When(x => x.Lane.HasValue);
        RuleFor(x => x.BibNumber).MaximumLength(ValidationLimits.BibNumberLength);
        RuleFor(x => x.DetectedFinishTimestampUtc).NotEmpty()
            .Must(value => value.Kind == DateTimeKind.Utc).WithMessage("DetectedFinishTimestampUtc must be UTC.");
        RuleFor(x => x.RawTimeSeconds).GreaterThan(0);
        RuleFor(x => x.AdjustedTimeSeconds).GreaterThan(0);
        RuleFor(x => x.DetectionMethod).IsInEnum();
        RuleFor(x => x.ConfidenceScore).InclusiveBetween(0, 100).When(x => x.ConfidenceScore.HasValue);
        RuleFor(x => x.FrameNumber).InclusiveBetween(0, int.MaxValue).When(x => x.FrameNumber.HasValue);
    }
}

public sealed class UpdateRawTimingResultRequestValidator : AbstractValidator<UpdateRawTimingResultRequest>
{
    public UpdateRawTimingResultRequestValidator(IOptions<RequestValidationOptions> options)
    {
        var maximumLane = options.Value.MaximumLane;

        RuleFor(x => x.RawTimeSeconds).GreaterThan(0).When(x => x.RawTimeSeconds.HasValue);
        RuleFor(x => x.AdjustedTimeSeconds).GreaterThan(0).When(x => x.AdjustedTimeSeconds.HasValue);
        RuleFor(x => x.DetectionMethod).IsInEnum().When(x => x.DetectionMethod.HasValue);
        RuleFor(x => x.ConfidenceScore).InclusiveBetween(0, 100).When(x => x.ConfidenceScore.HasValue);
        RuleFor(x => x.Lane).InclusiveBetween(1, maximumLane).When(x => x.Lane.HasValue);
        RuleFor(x => x.BibNumber).MaximumLength(ValidationLimits.BibNumberLength);
        RuleFor(x => x.DetectedFinishTimestampUtc)
            .Must(value => value!.Value.Kind == DateTimeKind.Utc).WithMessage("DetectedFinishTimestampUtc must be UTC.")
            .When(x => x.DetectedFinishTimestampUtc.HasValue);
        RuleFor(x => x.FrameNumber).InclusiveBetween(0, int.MaxValue).When(x => x.FrameNumber.HasValue);
        RuleFor(x => x.ReviewerNote).MaximumLength(ValidationLimits.ReviewerNoteLength);
    }
}

public sealed class RejectRawTimingResultRequestValidator : AbstractValidator<RejectRawTimingResultRequest>
{
    public RejectRawTimingResultRequestValidator()
    {
        RuleFor(x => x.ReviewerNote).NotEmpty().MaximumLength(ValidationLimits.ReviewerNoteLength);
    }
}

public sealed class SyncItemRequestValidator : AbstractValidator<SyncItemRequest>
{
    public SyncItemRequestValidator()
    {
        RuleFor(x => x.ClientGeneratedId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(ValidationLimits.EntityTypeLength);
        RuleFor(x => x.PayloadJson).NotEmpty()
            .MaximumLength(ValidationLimits.PayloadJsonLength)
            .Must(JsonValidation.IsValid).WithMessage("PayloadJson must contain valid JSON.");
    }
}

public sealed class SyncBatchRequestValidator : AbstractValidator<SyncBatchRequest>
{
    public SyncBatchRequestValidator(
        IOptions<RequestValidationOptions> options,
        IValidator<SyncItemRequest> itemValidator)
    {
        RuleFor(x => x.TimingSessionId).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.Items).NotNull().NotEmpty()
            .Must(items => items is null || items.Count <= options.Value.MaximumSyncBatchItems)
            .WithMessage($"Items must contain no more than {options.Value.MaximumSyncBatchItems} entries.");
        RuleForEach(x => x.Items).SetValidator(itemValidator);
    }
}
