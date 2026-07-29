using FluentValidation;
using Microsoft.Extensions.Options;
using TraficoTiming.Api.Models.Requests;
using TraficoTiming.Api.Validation;
using TraficoTiming.Database.Enums;
using Xunit;

namespace TraficoTiming.Tests;

public class RequestValidatorTests
{
    private static readonly IOptions<RequestValidationOptions> Options =
        Microsoft.Extensions.Options.Options.Create(new RequestValidationOptions
        {
            MaximumLane = 12,
            MaximumSyncBatchItems = 2
        });

    private static readonly DateTime UtcNow =
        new(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateTimingSession_AcceptsValidRequest()
    {
        var request = new CreateTimingSessionRequest(
            Guid.NewGuid(), Guid.NewGuid(), 1, "Final", Guid.NewGuid());

        Assert.True((await new CreateTimingSessionRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task CreateTimingSession_RejectsInvalidRequest()
    {
        var request = new CreateTimingSessionRequest(
            Guid.Empty, Guid.Empty, 0, new string('x', 51), Guid.Empty);

        var result = await new CreateTimingSessionRequestValidator().ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(request.MeetId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(request.Round));
    }

    [Fact]
    public async Task PairDevice_AcceptsValidRequest()
    {
        var request = new PairDeviceRequest
        {
            DeviceName = "Starter phone",
            DeviceType = "Android",
            DeviceRole = DeviceRole.Starter,
            ConnectionType = ConnectionType.Bluetooth,
            BatteryLevel = 80
        };

        Assert.True((await new PairDeviceRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task PairDevice_RejectsInvalidRequest()
    {
        var request = new PairDeviceRequest
        {
            DeviceName = "",
            DeviceType = new string('x', 51),
            DeviceRole = (DeviceRole)999,
            ConnectionType = (ConnectionType)999,
            BatteryLevel = 101
        };

        Assert.False((await new PairDeviceRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task ClockSync_AcceptsValidRequest()
    {
        var request = new ClockSyncRequest
        {
            StarterDeviceId = Guid.NewGuid(),
            FinishDeviceId = Guid.NewGuid(),
            OffsetMs = -12.5m,
            RoundTripDelayMs = 25,
            DriftMs = null,
            SyncQualityScore = 95
        };

        Assert.True((await new ClockSyncRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task ClockSync_RejectsInvalidRequest()
    {
        var id = Guid.NewGuid();
        var request = new ClockSyncRequest
        {
            StarterDeviceId = id,
            FinishDeviceId = id,
            OffsetMs = ValidationLimits.MaximumClockOffsetMs + 1,
            RoundTripDelayMs = -1,
            DriftMs = ValidationLimits.MaximumDriftMs + 1,
            SyncQualityScore = 101
        };

        Assert.False((await new ClockSyncRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task StartRace_AcceptsValidRequest()
    {
        var request = new StartRaceRequest
        {
            StarterDeviceId = Guid.NewGuid(),
            StartTimestampUtc = UtcNow,
            StartMethod = StartMethod.GunButton
        };

        Assert.True((await new StartRaceRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task StartRace_RejectsInvalidRequest()
    {
        var request = new StartRaceRequest
        {
            StarterDeviceId = Guid.Empty,
            StartTimestampUtc = DateTime.SpecifyKind(UtcNow, DateTimeKind.Local),
            StartMethod = (StartMethod)999
        };

        Assert.False((await new StartRaceRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task CreateFinishCapture_AcceptsValidRequest()
    {
        var request = new CreateFinishCaptureRequest(
            Guid.NewGuid(), "local-1", 59.94m, "1920x1080", UtcNow,
            UtcNow.AddSeconds(10), """{"line":42}""", "https://example.test/video.mp4");

        Assert.True((await new CreateFinishCaptureRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task CreateFinishCapture_RejectsInvalidRequest()
    {
        var request = new CreateFinishCaptureRequest(
            Guid.Empty, new string('x', 201), 0, "", UtcNow,
            UtcNow, "{bad json", new string('x', 1001));

        Assert.False((await new CreateFinishCaptureRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task CreateRawTimingResult_AcceptsValidRequest()
    {
        var request = ValidRawResult();

        Assert.True((await new CreateRawTimingResultRequestValidator(Options).ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task CreateRawTimingResult_RejectsInvalidRequest()
    {
        var request = new CreateRawTimingResultRequest
        {
            AthleteId = Guid.Empty,
            Lane = 13,
            BibNumber = new string('x', 51),
            DetectedFinishTimestampUtc = DateTime.SpecifyKind(UtcNow, DateTimeKind.Local),
            RawTimeSeconds = 0,
            AdjustedTimeSeconds = -1,
            DetectionMethod = (DetectionMethod)999,
            ConfidenceScore = 101,
            FrameNumber = (long)int.MaxValue + 1
        };

        Assert.False((await new CreateRawTimingResultRequestValidator(Options).ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task CreateRawTimingResult_RequiresAnIdentifier()
    {
        var request = ValidRawResult();
        request.AthleteId = null;
        request.Lane = null;
        request.BibNumber = " ";

        Assert.False((await new CreateRawTimingResultRequestValidator(Options).ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task UpdateRawTimingResult_AcceptsEmptyAndValidSuppliedFields()
    {
        var validator = new UpdateRawTimingResultRequestValidator(Options);
        var empty = new UpdateRawTimingResultRequest(null, null);
        var supplied = new UpdateRawTimingResultRequest(
            10, "reviewed", 10, DetectionMethod.AI, 90, 2, "B12", UtcNow, 0);

        Assert.True((await validator.ValidateAsync(empty)).IsValid);
        Assert.True((await validator.ValidateAsync(supplied)).IsValid);
    }

    [Fact]
    public async Task UpdateRawTimingResult_RejectsInvalidSuppliedFields()
    {
        var request = new UpdateRawTimingResultRequest(
            0, new string('x', 1001), -1, (DetectionMethod)999, -1, 0,
            new string('x', 51), DateTime.SpecifyKind(UtcNow, DateTimeKind.Unspecified), -1);

        Assert.False((await new UpdateRawTimingResultRequestValidator(Options).ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task RejectRawTimingResult_AcceptsValidRequest()
    {
        var request = new RejectRawTimingResultRequest { ReviewerNote = "False start." };

        Assert.True((await new RejectRawTimingResultRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task RejectRawTimingResult_RejectsInvalidRequest()
    {
        var request = new RejectRawTimingResultRequest { ReviewerNote = "" };

        Assert.False((await new RejectRawTimingResultRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task SyncItem_AcceptsValidRequest()
    {
        var request = new SyncItemRequest(Guid.NewGuid(), "RawTimingResult", """{"lane":1}""");

        Assert.True((await new SyncItemRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task SyncItem_RejectsInvalidRequest()
    {
        var request = new SyncItemRequest(Guid.Empty, new string('x', 101), "{bad json");

        Assert.False((await new SyncItemRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task SyncBatch_AcceptsValidRequest()
    {
        var request = new SyncBatchRequest(
            Guid.NewGuid(), Guid.NewGuid(),
            [new SyncItemRequest(Guid.NewGuid(), "Result", "{}")]);

        Assert.True((await SyncBatchValidator().ValidateAsync(request)).IsValid);
    }

    [Fact]
    public async Task SyncBatch_RejectsInvalidRequestAndTooManyItems()
    {
        var items = Enumerable.Range(0, 3)
            .Select(_ => new SyncItemRequest(Guid.NewGuid(), "Result", "{}")).ToList();
        var request = new SyncBatchRequest(Guid.Empty, Guid.Empty, items);

        Assert.False((await SyncBatchValidator().ValidateAsync(request)).IsValid);
    }

    private static CreateRawTimingResultRequest ValidRawResult() => new()
    {
        AthleteId = Guid.NewGuid(),
        Lane = 4,
        BibNumber = "B12",
        DetectedFinishTimestampUtc = UtcNow,
        RawTimeSeconds = 10.12m,
        AdjustedTimeSeconds = 10.11m,
        DetectionMethod = DetectionMethod.Hybrid,
        ConfidenceScore = 98,
        FrameNumber = 42
    };

    private static SyncBatchRequestValidator SyncBatchValidator()
    {
        IValidator<SyncItemRequest> itemValidator = new SyncItemRequestValidator();
        return new SyncBatchRequestValidator(Options, itemValidator);
    }
}
