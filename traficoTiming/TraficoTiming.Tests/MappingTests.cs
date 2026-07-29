using Moq;
using TraficoTiming.Api.Controllers;
using TraficoTiming.Api.Models.Requests;
using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Implementations;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;
using Xunit;

namespace TraficoTiming.Tests;

public class MappingTests
{
    private readonly Mock<IAuditLogService> _auditLog = new();

    [Fact]
    public async Task CreateSession_PreservesNullRound()
    {
        TimingSession? persisted = null;
        var repository = new Mock<ITimingSessionRepository>();
        repository
            .Setup(x => x.CreateAsync(It.IsAny<TimingSession>(), It.IsAny<CancellationToken>()))
            .Callback<TimingSession, CancellationToken>((entity, _) => persisted = entity)
            .ReturnsAsync((TimingSession entity, CancellationToken _) => entity);

        var service = new TimingSessionService(repository.Object, _auditLog.Object);

        await service.CreateSessionAsync(new CreateTimingSessionModel { Round = null });

        Assert.NotNull(persisted);
        Assert.Null(persisted.Round);
    }

    [Fact]
    public async Task PairDevice_PersistsBatteryLevel()
    {
        var sessionId = Guid.NewGuid();
        TimingDevice? persisted = null;
        var deviceRepository = new Mock<ITimingDeviceRepository>();
        var sessionRepository = new Mock<ITimingSessionRepository>();
        sessionRepository.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TimingSession { Id = sessionId });
        deviceRepository.Setup(x => x.CreateAsync(It.IsAny<TimingDevice>(), It.IsAny<CancellationToken>()))
            .Callback<TimingDevice, CancellationToken>((entity, _) => persisted = entity)
            .ReturnsAsync((TimingDevice entity, CancellationToken _) => entity);

        var service = new DevicePairingService(deviceRepository.Object, sessionRepository.Object, _auditLog.Object);

        await service.PairDeviceAsync(new PairDeviceModel
        {
            TimingSessionId = sessionId,
            DeviceName = "Starter",
            DeviceType = "Phone",
            DeviceRole = DeviceRole.Starter,
            BatteryLevel = 73
        });

        Assert.NotNull(persisted);
        Assert.Equal(73, persisted.BatteryLevel);
    }

    [Fact]
    public async Task ClockSync_PreservesDecimalsNullDriftAndSuppliedQuality()
    {
        ClockSyncRecord? persisted = null;
        var clockRepository = new Mock<IClockSyncRecordRepository>();
        clockRepository.Setup(x => x.CreateAsync(It.IsAny<ClockSyncRecord>(), It.IsAny<CancellationToken>()))
            .Callback<ClockSyncRecord, CancellationToken>((entity, _) => persisted = entity)
            .ReturnsAsync((ClockSyncRecord entity, CancellationToken _) => entity);
        var sessionRepository = new Mock<ITimingSessionRepository>();
        var service = new ClockSyncService(clockRepository.Object, sessionRepository.Object, _auditLog.Object);

        await service.SyncClocksAsync(new ClockSyncModel
        {
            TimingSessionId = Guid.NewGuid(),
            StarterDeviceId = Guid.NewGuid(),
            FinishDeviceId = Guid.NewGuid(),
            OffsetMs = 1.234m,
            RoundTripDelayMs = 5.678m,
            DriftMs = null,
            SyncQualityScore = 69.25m
        });

        Assert.NotNull(persisted);
        Assert.Equal(1.234m, persisted.OffsetMs);
        Assert.Equal(5.678m, persisted.RoundTripDelayMs);
        Assert.Null(persisted.DriftMs);
        Assert.Equal(69.25m, persisted.SyncQualityScore);
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("100.01")]
    public async Task ClockSync_RejectsOutOfRangeSuppliedQuality(string value)
    {
        var service = new ClockSyncService(
            Mock.Of<IClockSyncRecordRepository>(),
            Mock.Of<ITimingSessionRepository>(),
            _auditLog.Object);
        var model = new ClockSyncModel { SyncQualityScore = decimal.Parse(value) };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SyncClocksAsync(model));
    }

    [Fact]
    public async Task StartRace_PersistsClientTimestamp()
    {
        var sessionId = Guid.NewGuid();
        var timestamp = new DateTime(2026, 7, 29, 12, 34, 56, DateTimeKind.Utc);
        RaceStartEvent? persisted = null;
        var raceRepository = new Mock<IRaceStartEventRepository>();
        raceRepository.Setup(x => x.CreateAsync(It.IsAny<RaceStartEvent>(), It.IsAny<CancellationToken>()))
            .Callback<RaceStartEvent, CancellationToken>((entity, _) => persisted = entity)
            .ReturnsAsync((RaceStartEvent entity, CancellationToken _) => entity);
        var sessionRepository = new Mock<ITimingSessionRepository>();
        sessionRepository.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TimingSession { Id = sessionId, Status = SessionStatus.Ready });
        var deviceRepository = new Mock<ITimingDeviceRepository>();
        deviceRepository.Setup(x => x.RoleExistsInSessionAsync(sessionId, It.IsAny<DeviceRole>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new RaceStartService(
            raceRepository.Object, sessionRepository.Object, deviceRepository.Object, _auditLog.Object);

        var result = await service.StartRaceAsync(new StartRaceModel
        {
            TimingSessionId = sessionId,
            StarterDeviceId = Guid.NewGuid(),
            StartTimestampUtc = timestamp
        });

        Assert.Equal(timestamp, persisted!.StartTimestampUtc);
        Assert.Equal(timestamp, result.StartTimestampUtc);
    }

    [Fact]
    public async Task FinishCapture_PreservesDecimalFrameRateAndBothTimestamps()
    {
        FinishCapture? persisted = null;
        var repository = new Mock<IFinishCaptureRepository>();
        repository.Setup(x => x.CreateAsync(It.IsAny<FinishCapture>(), It.IsAny<CancellationToken>()))
            .Callback<FinishCapture, CancellationToken>((entity, _) => persisted = entity)
            .ReturnsAsync((FinishCapture entity, CancellationToken _) => entity);
        var service = new FinishCaptureService(repository.Object, _auditLog.Object);
        var started = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);
        var ended = started.AddSeconds(12);

        await service.CreateCaptureAsync(new CreateFinishCaptureModel
        {
            FrameRate = 59.940m,
            RecordingStartedAtUtc = started,
            RecordingEndedAtUtc = ended
        });

        Assert.NotNull(persisted);
        Assert.Equal(59.940m, persisted.FrameRate);
        Assert.Equal(started, persisted.RecordingStartedAtUtc);
        Assert.Equal(ended, persisted.RecordingEndedAtUtc);
    }

    [Fact]
    public async Task RawResult_PreservesDecimalValuesAndNullLane()
    {
        RawTimingResult? persisted = null;
        var repository = new Mock<IRawTimingResultRepository>();
        repository.Setup(x => x.CreateRawResultAsync(It.IsAny<RawTimingResult>(), It.IsAny<CancellationToken>()))
            .Callback<RawTimingResult, CancellationToken>((entity, _) => persisted = entity)
            .ReturnsAsync((RawTimingResult entity, CancellationToken _) => entity);
        var service = new RawTimingResultService(repository.Object, _auditLog.Object);

        await service.CreateResultAsync(new CreateRawTimingResultModel
        {
            Lane = null,
            RawTimeSeconds = 10.1234m,
            AdjustedTimeSeconds = 10.1235m,
            ConfidenceScore = 99.99m
        });

        Assert.NotNull(persisted);
        Assert.Null(persisted.Lane);
        Assert.Equal(10.1234m, persisted.RawTimeSeconds);
        Assert.Equal(10.1235m, persisted.AdjustedTimeSeconds);
        Assert.Equal(99.99m, persisted.ConfidenceScore);
        repository.Verify(
            x => x.LaneExistsInSessionAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RawResult_ValidatesFrameNumberBeforePersistenceConversion()
    {
        var repository = new Mock<IRawTimingResultRepository>();
        var service = new RawTimingResultService(repository.Object, _auditLog.Object);
        var model = new CreateRawTimingResultModel
        {
            RawTimeSeconds = 1m,
            FrameNumber = (long)int.MaxValue + 1
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.CreateResultAsync(model));
        repository.Verify(
            x => x.CreateRawResultAsync(It.IsAny<RawTimingResult>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Controllers_PreserveRequestValuesInServiceModels()
    {
        ClockSyncModel? clockModel = null;
        var clockService = new Mock<IClockSyncService>();
        clockService.Setup(x => x.SyncClocksAsync(It.IsAny<ClockSyncModel>(), It.IsAny<CancellationToken>()))
            .Callback<ClockSyncModel, CancellationToken>((model, _) => clockModel = model)
            .ReturnsAsync(new ClockSyncRecord());
        var clockController = new ClockSyncController(clockService.Object);
        await clockController.SyncClocks(Guid.NewGuid(), new ClockSyncRequest
        {
            OffsetMs = 1.111m,
            RoundTripDelayMs = 2.222m,
            DriftMs = null,
            SyncQualityScore = 88.88m
        }, CancellationToken.None);

        Assert.Equal(1.111m, clockModel!.OffsetMs);
        Assert.Equal(2.222m, clockModel.RoundTripDelayMs);
        Assert.Null(clockModel.DriftMs);
        Assert.Equal(88.88m, clockModel.SyncQualityScore);

        CreateFinishCaptureModel? captureModel = null;
        var captureService = new Mock<IFinishCaptureService>();
        captureService.Setup(x => x.CreateCaptureAsync(It.IsAny<CreateFinishCaptureModel>(), It.IsAny<CancellationToken>()))
            .Callback<CreateFinishCaptureModel, CancellationToken>((model, _) => captureModel = model)
            .ReturnsAsync(new FinishCapture());
        var captureController = new FinishCapturesController(captureService.Object);
        var ended = DateTime.UtcNow;
        await captureController.CreateCapture(Guid.NewGuid(),
            new CreateFinishCaptureRequest(Guid.NewGuid(), null, 29.970m, null, ended.AddSeconds(-1), ended, null),
            CancellationToken.None);

        Assert.Equal(29.970m, captureModel!.FrameRate);
        Assert.Equal(ended, captureModel.RecordingEndedAtUtc);
    }
}
