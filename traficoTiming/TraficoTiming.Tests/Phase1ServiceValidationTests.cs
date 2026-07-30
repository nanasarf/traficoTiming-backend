using Moq;
using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Implementations;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;
using Xunit;

namespace TraficoTiming.Tests;

public class Phase1ServiceValidationTests
{
    private readonly Mock<IAuditLogService> _audit = new();

    [Fact]
    public async Task ClockSync_ValidSameSessionDevices_UpdatesDevicesAndStoresPoorQualityWithoutReady()
    {
        var f = ClockFixture(SessionStatus.DevicesPaired, 69m);

        await f.Service.SyncClocksAsync(f.Model);

        Assert.Equal(69m, f.Session.SyncQualityScore);
        Assert.Equal(SessionStatus.DevicesPaired, f.Session.Status);
        Assert.Equal(0, f.Starter.ClockOffsetMs);
        Assert.Equal(0, f.Starter.ClockDriftMs);
        Assert.Equal(f.Model.OffsetMs, f.Finish.ClockOffsetMs);
        Assert.Equal(f.Model.DriftMs, f.Finish.ClockDriftMs);
        Assert.NotNull(f.Starter.LastSyncedAtUtc);
        Assert.Equal(f.Starter.LastSyncedAtUtc, f.Finish.LastSyncedAtUtc);
        f.Clock.Verify(x => x.CreateAsync(It.IsAny<ClockSyncRecord>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClockSync_AcceptableQuality_PersistsQualityAndMarksReady()
    {
        var f = ClockFixture(SessionStatus.DevicesPaired, 70m);

        await f.Service.SyncClocksAsync(f.Model);

        Assert.Equal(70m, f.Session.SyncQualityScore);
        Assert.Equal(SessionStatus.Ready, f.Session.Status);
    }

    [Fact]
    public async Task ClockSync_PoorQuality_DoesNotDowngradeReadySession()
    {
        var f = ClockFixture(SessionStatus.Ready, 20m);

        await f.Service.SyncClocksAsync(f.Model);

        Assert.Equal(SessionStatus.Ready, f.Session.Status);
        Assert.Equal(20m, f.Session.SyncQualityScore);
    }

    [Theory]
    [InlineData("starter-session")]
    [InlineData("finish-session")]
    [InlineData("starter-role")]
    [InlineData("finish-role")]
    [InlineData("starter-disconnected")]
    [InlineData("finish-disconnected")]
    [InlineData("running")]
    public async Task ClockSync_InvalidOwnershipRoleConnectivityOrState_ConflictsWithoutSaving(string scenario)
    {
        var f = ClockFixture(
            scenario == "running" ? SessionStatus.Running : SessionStatus.DevicesPaired,
            90m);
        if (scenario == "starter-session") f.Starter.TimingSessionId = Guid.NewGuid();
        if (scenario == "finish-session") f.Finish.TimingSessionId = Guid.NewGuid();
        if (scenario == "starter-role") f.Starter.DeviceRole = DeviceRole.Finish;
        if (scenario == "finish-role") f.Finish.DeviceRole = DeviceRole.Backup;
        if (scenario == "starter-disconnected") f.Starter.Status = DeviceStatus.Disconnected;
        if (scenario == "finish-disconnected") f.Finish.Status = DeviceStatus.Disconnected;

        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.SyncClocksAsync(f.Model));
        f.Clock.Verify(x => x.CreateAsync(It.IsAny<ClockSyncRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RaceStart_ValidReadySessionAcceptedSync_SavesClientTimestampAndRuns()
    {
        var f = RaceFixture();

        var result = await f.Service.StartRaceAsync(f.Model);

        Assert.Equal(f.Model.StartTimestampUtc, result.StartTimestampUtc);
        Assert.Equal(SessionStatus.Running, f.Session.Status);
        Assert.Equal(f.Model.StartTimestampUtc, f.Session.StartedAtUtc);
        f.Races.Verify(x => x.CreateAsync(It.IsAny<RaceStartEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("not-ready")]
    [InlineData("unknown-starter")]
    [InlineData("cross-session")]
    [InlineData("wrong-role")]
    [InlineData("disconnected-starter")]
    [InlineData("no-finish")]
    [InlineData("no-sync")]
    [InlineData("duplicate")]
    public async Task RaceStart_InvalidPrecondition_RejectsWithoutSaving(string scenario)
    {
        var f = RaceFixture();
        if (scenario == "not-ready") f.Session.Status = SessionStatus.DevicesPaired;
        if (scenario == "unknown-starter")
            f.Devices.Setup(x => x.GetByIdAsync(f.Starter.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimingDevice?)null);
        if (scenario == "cross-session") f.Starter.TimingSessionId = Guid.NewGuid();
        if (scenario == "wrong-role") f.Starter.DeviceRole = DeviceRole.Finish;
        if (scenario == "disconnected-starter") f.Starter.Status = DeviceStatus.Disconnected;
        if (scenario == "no-finish")
            f.Devices.Setup(x => x.GetFinishDevicesAsync(f.Session.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
        if (scenario == "no-sync")
            f.Clock.Setup(x => x.ExistsAcceptedSyncAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        if (scenario == "duplicate")
            f.Races.Setup(x => x.ExistsForSessionAsync(f.Session.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

        var exception = await Record.ExceptionAsync(() => f.Service.StartRaceAsync(f.Model));

        Assert.True(exception is InvalidOperationException or KeyNotFoundException);
        f.Races.Verify(x => x.CreateAsync(It.IsAny<RaceStartEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RaceStart_UnknownSession_ReturnsNotFound()
    {
        var f = RaceFixture();
        f.Sessions.Setup(x => x.GetByIdAsync(f.Session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimingSession?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => f.Service.StartRaceAsync(f.Model));
    }

    [Fact]
    public async Task FinishCapture_ValidFinishAndPreRollInterval_Succeeds()
    {
        var f = CaptureFixture();
        f.Model.RecordingStartedAtUtc = f.RaceStart.StartTimestampUtc.AddSeconds(-2);
        f.Model.RecordingEndedAtUtc = f.RaceStart.StartTimestampUtc.AddSeconds(4);

        var result = await f.Service.CreateCaptureAsync(f.Model);

        Assert.Equal(f.Model.RecordingStartedAtUtc, result.RecordingStartedAtUtc);
        f.Captures.Verify(x => x.CreateAsync(It.IsAny<FinishCapture>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("not-running")]
    [InlineData("cross-session")]
    [InlineData("wrong-role")]
    [InlineData("disconnected")]
    [InlineData("missing-start")]
    [InlineData("ends-before-start")]
    public async Task FinishCapture_InvalidPrecondition_RejectsWithoutSaving(string scenario)
    {
        var f = CaptureFixture();
        if (scenario == "not-running") f.Session.Status = SessionStatus.Ready;
        if (scenario == "cross-session") f.Finish.TimingSessionId = Guid.NewGuid();
        if (scenario == "wrong-role") f.Finish.DeviceRole = DeviceRole.Backup;
        if (scenario == "disconnected") f.Finish.Status = DeviceStatus.Disconnected;
        if (scenario == "missing-start")
            f.Races.Setup(x => x.GetLatestBySessionIdAsync(f.Session.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RaceStartEvent?)null);
        if (scenario == "ends-before-start")
        {
            f.Model.RecordingStartedAtUtc = f.RaceStart.StartTimestampUtc.AddSeconds(-5);
            f.Model.RecordingEndedAtUtc = f.RaceStart.StartTimestampUtc.AddMilliseconds(-1);
        }

        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.CreateCaptureAsync(f.Model));
        f.Captures.Verify(x => x.CreateAsync(It.IsAny<FinishCapture>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FinishCapture_UnknownDevice_ReturnsNotFound()
    {
        var f = CaptureFixture();
        f.Devices.Setup(x => x.GetByIdAsync(f.Finish.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimingDevice?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => f.Service.CreateCaptureAsync(f.Model));
    }

    [Fact]
    public async Task Disconnect_RequiredDeviceBeforeStart_InvalidatesReady()
    {
        var f = DisconnectFixture(SessionStatus.Ready, DeviceRole.Starter);

        await f.Service.DisconnectDeviceAsync(f.Target.Id);

        Assert.Equal(SessionStatus.DevicesPaired, f.Session.Status);
    }

    [Fact]
    public async Task Disconnect_OptionalBackup_DoesNotInvalidateReady()
    {
        var f = DisconnectFixture(SessionStatus.Ready, DeviceRole.Backup);

        await f.Service.DisconnectDeviceAsync(f.Target.Id);

        Assert.Equal(SessionStatus.Ready, f.Session.Status);
        f.Sessions.Verify(x => x.UpdateAsync(It.IsAny<TimingSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Disconnect_RunningSession_IsNotTransitioned()
    {
        var f = DisconnectFixture(SessionStatus.Running, DeviceRole.Starter);

        await f.Service.DisconnectDeviceAsync(f.Target.Id);

        Assert.Equal(DeviceStatus.Disconnected, f.Target.Status);
        Assert.Equal(SessionStatus.Running, f.Session.Status);
    }

    private ClockTestFixture ClockFixture(SessionStatus status, decimal quality)
    {
        var session = new TimingSession { Id = Guid.NewGuid(), Status = status };
        var starter = Device(session.Id, DeviceRole.Starter);
        var finish = Device(session.Id, DeviceRole.Finish);
        var sessions = new Mock<ITimingSessionRepository>();
        var devices = new Mock<ITimingDeviceRepository>();
        var clock = new Mock<IClockSyncRecordRepository>();
        sessions.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        devices.Setup(x => x.GetByIdAsync(starter.Id, It.IsAny<CancellationToken>())).ReturnsAsync(starter);
        devices.Setup(x => x.GetByIdAsync(finish.Id, It.IsAny<CancellationToken>())).ReturnsAsync(finish);
        devices.Setup(x => x.GetDevicesBySessionAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([starter, finish]);
        clock.Setup(x => x.CreateAsync(It.IsAny<ClockSyncRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClockSyncRecord record, CancellationToken _) => record);
        var model = new ClockSyncModel
        {
            TimingSessionId = session.Id, StarterDeviceId = starter.Id, FinishDeviceId = finish.Id,
            OffsetMs = 1.25m, DriftMs = .2m, RoundTripDelayMs = 2m, SyncQualityScore = quality
        };
        return new(session, starter, finish, clock, model,
            new ClockSyncService(clock.Object, sessions.Object, devices.Object, _audit.Object));
    }

    private RaceTestFixture RaceFixture()
    {
        var session = new TimingSession { Id = Guid.NewGuid(), Status = SessionStatus.Ready };
        var starter = Device(session.Id, DeviceRole.Starter);
        var finish = Device(session.Id, DeviceRole.Finish);
        var sessions = new Mock<ITimingSessionRepository>();
        var devices = new Mock<ITimingDeviceRepository>();
        var races = new Mock<IRaceStartEventRepository>();
        var clock = new Mock<IClockSyncRecordRepository>();
        sessions.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        devices.Setup(x => x.GetByIdAsync(starter.Id, It.IsAny<CancellationToken>())).ReturnsAsync(starter);
        devices.Setup(x => x.GetFinishDevicesAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync([finish]);
        clock.Setup(x => x.ExistsAcceptedSyncAsync(
                session.Id, starter.Id, It.IsAny<IReadOnlyCollection<Guid>>(),
                ClockSyncService.ReadinessThreshold, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        races.Setup(x => x.CreateAsync(It.IsAny<RaceStartEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RaceStartEvent race, CancellationToken _) => race);
        var model = new StartRaceModel
        {
            TimingSessionId = session.Id, StarterDeviceId = starter.Id,
            StartTimestampUtc = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc)
        };
        return new(session, starter, sessions, devices, races, clock, model,
            new RaceStartService(races.Object, sessions.Object, devices.Object, clock.Object, _audit.Object));
    }

    private CaptureTestFixture CaptureFixture()
    {
        var session = new TimingSession { Id = Guid.NewGuid(), Status = SessionStatus.Running };
        var finish = Device(session.Id, DeviceRole.Finish);
        var start = new RaceStartEvent
        {
            TimingSessionId = session.Id,
            StartTimestampUtc = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc)
        };
        var captures = new Mock<IFinishCaptureRepository>();
        var sessions = new Mock<ITimingSessionRepository>();
        var devices = new Mock<ITimingDeviceRepository>();
        var races = new Mock<IRaceStartEventRepository>();
        sessions.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        devices.Setup(x => x.GetByIdAsync(finish.Id, It.IsAny<CancellationToken>())).ReturnsAsync(finish);
        races.Setup(x => x.GetLatestBySessionIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(start);
        captures.Setup(x => x.CreateAsync(It.IsAny<FinishCapture>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinishCapture capture, CancellationToken _) => capture);
        var model = new CreateFinishCaptureModel
        {
            TimingSessionId = session.Id, FinishDeviceId = finish.Id,
            RecordingStartedAtUtc = start.StartTimestampUtc,
            RecordingEndedAtUtc = start.StartTimestampUtc.AddSeconds(2)
        };
        return new(session, finish, start, captures, devices, races, model,
            new FinishCaptureService(captures.Object, sessions.Object, devices.Object, races.Object, _audit.Object));
    }

    private DisconnectTestFixture DisconnectFixture(SessionStatus status, DeviceRole targetRole)
    {
        var session = new TimingSession { Id = Guid.NewGuid(), Status = status };
        var starter = Device(session.Id, DeviceRole.Starter);
        var finish = Device(session.Id, DeviceRole.Finish);
        var target = targetRole switch
        {
            DeviceRole.Starter => starter,
            DeviceRole.Finish => finish,
            _ => Device(session.Id, DeviceRole.Backup)
        };
        var devices = new Mock<ITimingDeviceRepository>();
        var sessions = new Mock<ITimingSessionRepository>();
        devices.Setup(x => x.GetByIdAsync(target.Id, It.IsAny<CancellationToken>())).ReturnsAsync(target);
        devices.Setup(x => x.GetDevicesBySessionAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([starter, finish, target]);
        sessions.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        return new(session, target, sessions,
            new DevicePairingService(devices.Object, sessions.Object, _audit.Object));
    }

    private static TimingDevice Device(Guid sessionId, DeviceRole role) => new()
    {
        Id = Guid.NewGuid(), TimingSessionId = sessionId, DeviceRole = role, Status = DeviceStatus.Connected
    };

    private sealed record ClockTestFixture(
        TimingSession Session, TimingDevice Starter, TimingDevice Finish,
        Mock<IClockSyncRecordRepository> Clock, ClockSyncModel Model, ClockSyncService Service);
    private sealed record RaceTestFixture(
        TimingSession Session, TimingDevice Starter, Mock<ITimingSessionRepository> Sessions,
        Mock<ITimingDeviceRepository> Devices, Mock<IRaceStartEventRepository> Races,
        Mock<IClockSyncRecordRepository> Clock, StartRaceModel Model, RaceStartService Service);
    private sealed record CaptureTestFixture(
        TimingSession Session, TimingDevice Finish, RaceStartEvent RaceStart,
        Mock<IFinishCaptureRepository> Captures, Mock<ITimingDeviceRepository> Devices,
        Mock<IRaceStartEventRepository> Races, CreateFinishCaptureModel Model, FinishCaptureService Service);
    private sealed record DisconnectTestFixture(
        TimingSession Session, TimingDevice Target, Mock<ITimingSessionRepository> Sessions,
        DevicePairingService Service);
}
