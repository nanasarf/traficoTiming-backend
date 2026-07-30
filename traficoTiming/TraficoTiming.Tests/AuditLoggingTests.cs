using System.Text.Json;
using Moq;
using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services;
using TraficoTiming.Services.Implementations;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;
using Xunit;

namespace TraficoTiming.Tests;

public class AuditLoggingTests
{
    [Fact]
    public async Task SessionActions_AreAudited_WithStatusContext()
    {
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new TimingSession
        {
            Id = sessionId, CreatedByUserId = userId, Status = SessionStatus.Ready
        };
        var sessions = new Mock<ITimingSessionRepository>();
        sessions.Setup(x => x.CreateAsync(It.IsAny<TimingSession>(), It.IsAny<CancellationToken>()))
            .Callback<TimingSession, CancellationToken>((value, _) => value.Id = sessionId)
            .ReturnsAsync((TimingSession value, CancellationToken _) => value);
        sessions.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        var audit = CaptureAudit();
        var service = new TimingSessionService(sessions.Object, audit.Mock.Object);

        await service.CreateSessionAsync(new CreateTimingSessionModel { CreatedByUserId = userId });
        await service.EndSessionAsync(sessionId);
        session.Status = SessionStatus.Ready;
        await service.CancelSessionAsync(sessionId);

        AssertActions(audit.Entries,
            AuditActions.SessionCreated,
            AuditActions.SessionCompleted,
            AuditActions.SessionCancelled);
        Assert.Equal(userId, audit.Entries[0].UserId);
        AssertJson(audit.Entries[1].Details, root =>
        {
            Assert.Equal("Ready", root.GetProperty("previousStatus").GetString());
            Assert.Equal("Completed", root.GetProperty("newStatus").GetString());
        });
    }

    [Fact]
    public async Task InvalidSessionOperation_DoesNotWriteSuccessAudit()
    {
        var sessionId = Guid.NewGuid();
        var sessions = new Mock<ITimingSessionRepository>();
        sessions.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TimingSession { Id = sessionId, Status = SessionStatus.Completed });
        var audit = CaptureAudit();
        var service = new TimingSessionService(sessions.Object, audit.Mock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EndSessionAsync(sessionId));

        Assert.Empty(audit.Entries);
    }

    [Fact]
    public async Task DeviceActions_AreAudited_AndDisconnectIncludesSessionTransition()
    {
        var sessionId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var session = new TimingSession { Id = sessionId, Status = SessionStatus.Created };
        var device = new TimingDevice
        {
            Id = deviceId, TimingSessionId = sessionId, DeviceRole = DeviceRole.Finish,
            ConnectionType = ConnectionType.WifiDirect, Status = DeviceStatus.Connected
        };
        var sessions = new Mock<ITimingSessionRepository>();
        sessions.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        var devices = new Mock<ITimingDeviceRepository>();
        devices.Setup(x => x.CreateAsync(It.IsAny<TimingDevice>(), It.IsAny<CancellationToken>()))
            .Callback<TimingDevice, CancellationToken>((value, _) => value.Id = deviceId)
            .ReturnsAsync((TimingDevice value, CancellationToken _) => value);
        devices.Setup(x => x.GetByIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        devices.Setup(x => x.RoleExistsInSessionAsync(sessionId, It.IsAny<DeviceRole>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        devices.Setup(x => x.GetDevicesBySessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new TimingDevice { DeviceRole = DeviceRole.Starter, Status = DeviceStatus.Connected },
                device
            ]);
        var audit = CaptureAudit();
        var service = new DevicePairingService(devices.Object, sessions.Object, audit.Mock.Object);

        await service.PairDeviceAsync(new PairDeviceModel
        {
            TimingSessionId = sessionId, DeviceRole = DeviceRole.Finish,
            ConnectionType = ConnectionType.WifiDirect
        });
        await service.UpdateHeartbeatAsync(deviceId, 88);
        session.Status = SessionStatus.Ready;
        await service.DisconnectDeviceAsync(deviceId);

        AssertActions(audit.Entries,
            AuditActions.DevicePaired,
            AuditActions.DeviceHeartbeatUpdated,
            AuditActions.DeviceDisconnected);
        AssertJson(audit.Entries[2].Details, root =>
        {
            Assert.Equal("Ready", root.GetProperty("previousStatus").GetString());
            Assert.Equal("DevicesPaired", root.GetProperty("newStatus").GetString());
            Assert.Equal("required device disconnected", root.GetProperty("reason").GetString());
        });
    }

    [Theory]
    [InlineData("85", true)]
    [InlineData("60", false)]
    public async Task ClockSync_IsAudited_AndOnlyAcceptedSyncMarksReady(string scoreText, bool expectsReady)
    {
        var score = decimal.Parse(scoreText);
        var sessionId = Guid.NewGuid();
        var starterId = Guid.NewGuid();
        var finishId = Guid.NewGuid();
        var session = new TimingSession { Id = sessionId, Status = SessionStatus.DevicesPaired };
        var sessions = new Mock<ITimingSessionRepository>();
        sessions.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var devices = new Mock<ITimingDeviceRepository>();
        devices.Setup(x => x.GetByIdAsync(starterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Device(starterId, sessionId, DeviceRole.Starter));
        devices.Setup(x => x.GetByIdAsync(finishId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Device(finishId, sessionId, DeviceRole.Finish));
        devices.Setup(x => x.GetDevicesBySessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Device(starterId, sessionId, DeviceRole.Starter), Device(finishId, sessionId, DeviceRole.Finish)]);
        var clock = new Mock<IClockSyncRecordRepository>();
        clock.Setup(x => x.CreateAsync(It.IsAny<ClockSyncRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClockSyncRecord value, CancellationToken _) => value);
        var audit = CaptureAudit();
        var service = new ClockSyncService(clock.Object, sessions.Object, devices.Object, audit.Mock.Object);

        await service.SyncClocksAsync(new ClockSyncModel
        {
            TimingSessionId = sessionId, StarterDeviceId = starterId,
            FinishDeviceId = finishId, SyncQualityScore = score
        });

        Assert.Contains(audit.Entries, x => x.Action == AuditActions.ClockSyncRecorded);
        Assert.Equal(expectsReady, audit.Entries.Any(x => x.Action == AuditActions.SessionMarkedReady));
    }

    [Fact]
    public async Task RaceStart_IsAudited_WithUtcStartTimestamp()
    {
        var sessionId = Guid.NewGuid();
        var starterId = Guid.NewGuid();
        var timestamp = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);
        var sessions = new Mock<ITimingSessionRepository>();
        sessions.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TimingSession { Id = sessionId, Status = SessionStatus.Ready });
        var devices = new Mock<ITimingDeviceRepository>();
        devices.Setup(x => x.GetByIdAsync(starterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Device(starterId, sessionId, DeviceRole.Starter));
        devices.Setup(x => x.GetFinishDevicesAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Device(Guid.NewGuid(), sessionId, DeviceRole.Finish)]);
        var clocks = new Mock<IClockSyncRecordRepository>();
        clocks.Setup(x => x.ExistsAcceptedSyncAsync(sessionId, starterId,
                It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var races = new Mock<IRaceStartEventRepository>();
        races.Setup(x => x.CreateAsync(It.IsAny<RaceStartEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RaceStartEvent value, CancellationToken _) => value);
        var audit = CaptureAudit();
        var service = new RaceStartService(races.Object, sessions.Object, devices.Object, clocks.Object, audit.Mock.Object);

        await service.StartRaceAsync(new StartRaceModel
        {
            TimingSessionId = sessionId, StarterDeviceId = starterId, StartTimestampUtc = timestamp
        });

        Assert.Single(audit.Entries, x => x.Action == AuditActions.RaceStarted);
        AssertJson(audit.Entries.Single().Details,
            root => Assert.EndsWith("Z", root.GetProperty("startTimestamp").GetString()));
    }

    [Fact]
    public async Task FinishCapture_CreateAndUploadUpdate_AreAudited()
    {
        var sessionId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var captureId = Guid.NewGuid();
        var started = DateTime.UtcNow.AddSeconds(-2);
        var capture = new FinishCapture
        {
            Id = captureId, TimingSessionId = sessionId, FinishDeviceId = deviceId,
            UploadStatus = UploadStatus.LocalOnly
        };
        var captures = new Mock<IFinishCaptureRepository>();
        captures.Setup(x => x.CreateAsync(It.IsAny<FinishCapture>(), It.IsAny<CancellationToken>()))
            .Callback<FinishCapture, CancellationToken>((value, _) => value.Id = captureId)
            .ReturnsAsync((FinishCapture value, CancellationToken _) => value);
        captures.Setup(x => x.GetByIdAsync(captureId, It.IsAny<CancellationToken>())).ReturnsAsync(capture);
        var sessions = new Mock<ITimingSessionRepository>();
        sessions.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TimingSession { Id = sessionId, Status = SessionStatus.Running });
        var devices = new Mock<ITimingDeviceRepository>();
        devices.Setup(x => x.GetByIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Device(deviceId, sessionId, DeviceRole.Finish));
        var races = new Mock<IRaceStartEventRepository>();
        races.Setup(x => x.GetLatestBySessionIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RaceStartEvent { StartTimestampUtc = started.AddSeconds(.5) });
        var audit = CaptureAudit();
        var service = new FinishCaptureService(captures.Object, sessions.Object, devices.Object, races.Object, audit.Mock.Object);

        await service.CreateCaptureAsync(new CreateFinishCaptureModel
        {
            TimingSessionId = sessionId, FinishDeviceId = deviceId,
            RecordingStartedAtUtc = started, RecordingEndedAtUtc = started.AddSeconds(1)
        });
        await service.UpdateUploadStatusAsync(captureId, UploadStatus.Uploaded);

        AssertActions(audit.Entries,
            AuditActions.FinishCaptureCreated,
            AuditActions.FinishCaptureUploadStatusUpdated);
    }

    [Fact]
    public async Task RawResultLifecycle_IsAudited_WithOldAndNewStatuses()
    {
        var sessionId = Guid.NewGuid();
        var resultId = Guid.NewGuid();
        var result = new RawTimingResult
        {
            Id = resultId, TimingSessionId = sessionId, Lane = 4, Status = RawResultStatus.Pending
        };
        var results = new Mock<IRawTimingResultRepository>();
        results.Setup(x => x.CreateRawResultAsync(It.IsAny<RawTimingResult>(), It.IsAny<CancellationToken>()))
            .Callback<RawTimingResult, CancellationToken>((value, _) => value.Id = resultId)
            .ReturnsAsync((RawTimingResult value, CancellationToken _) => value);
        results.Setup(x => x.GetRawResultByIdAsync(resultId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        var audit = CaptureAudit();
        var service = new RawTimingResultService(results.Object, audit.Mock.Object);

        await service.CreateResultAsync(new CreateRawTimingResultModel
        {
            TimingSessionId = sessionId, Lane = 4, RawTimeSeconds = 10
        });
        await service.UpdateResultAsync(new UpdateRawTimingResultModel { ResultId = resultId, AdjustedTimeSeconds = 9.9m });
        await service.SubmitResultAsync(resultId);
        result.Status = RawResultStatus.Reviewed;
        await service.RejectResultAsync(resultId, "review rejected");

        AssertActions(audit.Entries,
            AuditActions.RawResultCreated,
            AuditActions.RawResultUpdated,
            AuditActions.RawResultSubmitted,
            AuditActions.RawResultRejected);
        AssertJson(audit.Entries[2].Details, root =>
        {
            Assert.Equal("Reviewed", root.GetProperty("previousResultStatus").GetString());
            Assert.Equal("Submitted", root.GetProperty("newResultStatus").GetString());
        });
    }

    [Fact]
    public async Task SyncBatch_ReceiptAndCompletion_AreAudited()
    {
        var batch = new SyncBatch { Id = Guid.NewGuid(), TimingSessionId = Guid.NewGuid(), DeviceId = Guid.NewGuid() };
        var batches = BatchRepository(batch);
        var audit = CaptureAudit();
        var service = new OfflineSyncService(batches.Object, Mock.Of<ISyncItemRepository>(), audit.Mock.Object);

        await service.ProcessSyncBatchAsync(new SyncBatchModel
        {
            TimingSessionId = batch.TimingSessionId, DeviceId = batch.DeviceId
        });

        AssertActions(audit.Entries, AuditActions.SyncBatchReceived, AuditActions.SyncBatchCompleted);
    }

    [Fact]
    public async Task SyncBatch_Failure_IsAuditedWithoutInternalExceptionDetails()
    {
        var batch = new SyncBatch { Id = Guid.NewGuid(), TimingSessionId = Guid.NewGuid(), DeviceId = Guid.NewGuid() };
        var batches = BatchRepository(batch);
        var items = new Mock<ISyncItemRepository>();
        items.Setup(x => x.ClientGeneratedIdExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("secret database host and stack detail"));
        var audit = CaptureAudit();
        var service = new OfflineSyncService(batches.Object, items.Object, audit.Mock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessSyncBatchAsync(new SyncBatchModel
        {
            TimingSessionId = batch.TimingSessionId,
            DeviceId = batch.DeviceId,
            Items = [new SyncItemModel { ClientGeneratedId = Guid.NewGuid().ToString() }]
        }));

        AssertActions(audit.Entries, AuditActions.SyncBatchReceived, AuditActions.SyncBatchFailed);
        AssertJson(audit.Entries[1].Details, root =>
        {
            Assert.Equal("batch_processing", root.GetProperty("failureCategory").GetString());
            Assert.DoesNotContain("secret", root.GetRawText(), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("stack", root.GetRawText(), StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task AuditLogService_WritesValidJsonAndUtcTimestamp()
    {
        TimingAuditLog? persisted = null;
        var repository = new Mock<ITimingAuditLogRepository>();
        repository.Setup(x => x.CreateAsync(It.IsAny<TimingAuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<TimingAuditLog, CancellationToken>((value, _) => persisted = value)
            .ReturnsAsync((TimingAuditLog value, CancellationToken _) => value);
        var before = DateTime.UtcNow;

        await new AuditLogService(repository.Object).LogAsync(
            Guid.NewGuid(), AuditActions.SessionCreated, details: new { previousStatus = "None" });

        Assert.NotNull(persisted);
        using var _ = JsonDocument.Parse(persisted.DetailsJson!);
        Assert.Equal(DateTimeKind.Utc, persisted.CreatedAtUtc.Kind);
        Assert.InRange(persisted.CreatedAtUtc, before, DateTime.UtcNow);
    }

    private static TimingDevice Device(Guid id, Guid sessionId, DeviceRole role) => new()
    {
        Id = id, TimingSessionId = sessionId, DeviceRole = role, Status = DeviceStatus.Connected
    };

    private static Mock<ISyncBatchRepository> BatchRepository(SyncBatch batch)
    {
        var repository = new Mock<ISyncBatchRepository>();
        repository.Setup(x => x.CreateSyncBatchAsync(It.IsAny<SyncBatch>(), It.IsAny<CancellationToken>()))
            .Callback<SyncBatch, CancellationToken>((value, _) =>
            {
                value.Id = batch.Id;
                batch.ItemCount = value.ItemCount;
            })
            .ReturnsAsync((SyncBatch value, CancellationToken _) => value);
        repository.Setup(x => x.GetSyncBatchByIdAsync(batch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(batch);
        return repository;
    }

    private static (Mock<IAuditLogService> Mock, List<AuditEntry> Entries) CaptureAudit()
    {
        var entries = new List<AuditEntry>();
        var mock = new Mock<IAuditLogService>();
        mock.Setup(x => x.LogAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
                It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, Guid?, Guid?, object?, CancellationToken>(
                (sessionId, action, deviceId, userId, details, _) =>
                    entries.Add(new AuditEntry(sessionId, action, deviceId, userId, details)))
            .Returns(Task.CompletedTask);
        return (mock, entries);
    }

    private static void AssertActions(IReadOnlyList<AuditEntry> entries, params string[] actions) =>
        Assert.Equal(actions, entries.Select(x => x.Action));

    private static void AssertJson(object? details, Action<JsonElement> assertion)
    {
        var json = JsonSerializer.Serialize(details, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);
        assertion(document.RootElement);
    }

    private sealed record AuditEntry(
        Guid SessionId, string Action, Guid? DeviceId, Guid? UserId, object? Details);
}
