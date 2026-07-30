using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Implementations;

public class FinishCaptureService(
    IFinishCaptureRepository captureRepo,
    ITimingSessionRepository sessionRepo,
    ITimingDeviceRepository deviceRepo,
    IRaceStartEventRepository raceStartRepo,
    IAuditLogService auditLog) : IFinishCaptureService
{
    public async Task<FinishCapture> CreateCaptureAsync(CreateFinishCaptureModel model, CancellationToken ct = default)
    {
        var session = await sessionRepo.GetByIdAsync(model.TimingSessionId, ct)
            ?? throw new KeyNotFoundException($"Session {model.TimingSessionId} not found.");
        if (session.Status != SessionStatus.Running)
            throw new InvalidOperationException("Finish capture requires a Running session.");

        var finish = await deviceRepo.GetByIdAsync(model.FinishDeviceId, ct)
            ?? throw new KeyNotFoundException($"Device {model.FinishDeviceId} not found.");
        if (finish.TimingSessionId != model.TimingSessionId)
            throw new InvalidOperationException("The Finish device does not belong to this session.");
        if (finish.DeviceRole != DeviceRole.Finish)
            throw new InvalidOperationException("The selected device does not have the Finish role.");
        if (finish.Status != DeviceStatus.Connected)
            throw new InvalidOperationException("The Finish device must be connected.");

        if (model.RecordingStartedAtUtc is null)
            throw new ArgumentException("RecordingStartedAtUtc is required.", nameof(model.RecordingStartedAtUtc));
        if (model.RecordingEndedAtUtc <= model.RecordingStartedAtUtc.Value)
            throw new ArgumentException("RecordingEndedAtUtc must be later than RecordingStartedAtUtc.", nameof(model.RecordingEndedAtUtc));

        var raceStart = await raceStartRepo.GetLatestBySessionIdAsync(model.TimingSessionId, ct)
            ?? throw new InvalidOperationException("A race start event is required before finish capture.");
        // Pre-roll is allowed: recording may begin before the race, but it must continue past the start.
        if (model.RecordingEndedAtUtc <= raceStart.StartTimestampUtc)
            throw new InvalidOperationException("The recording interval must end after the race start.");

        var capture = new FinishCapture
        {
            TimingSessionId               = model.TimingSessionId,
            FinishDeviceId                = model.FinishDeviceId,
            LocalFileId                   = model.LocalFileId,
            FrameRate                     = model.FrameRate ?? 0,
            Resolution                    = model.Resolution ?? string.Empty,
            RecordingStartedAtUtc         = model.RecordingStartedAtUtc.Value,
            RecordingEndedAtUtc           = model.RecordingEndedAtUtc,
            FinishLineCalibrationDataJson = model.FinishLineCalibrationDataJson,
            UploadStatus                  = UploadStatus.LocalOnly
        };
        await captureRepo.CreateAsync(capture, ct);
        await auditLog.LogAsync(capture.TimingSessionId, AuditActions.FinishCaptureCreated, capture.FinishDeviceId,
            details: new
            {
                captureId = capture.Id,
                recordingStartedAtUtc = capture.RecordingStartedAtUtc,
                recordingEndedAtUtc = capture.RecordingEndedAtUtc
            }, ct: ct);
        return capture;
    }

    public async Task<IReadOnlyList<FinishCapture>> GetCapturesAsync(Guid sessionId, CancellationToken ct = default) =>
        (await captureRepo.GetBySessionIdAsync(sessionId, ct)).AsReadOnly();

    public async Task UpdateUploadStatusAsync(Guid captureId, UploadStatus status, CancellationToken ct = default)
    {
        var capture = await captureRepo.GetByIdAsync(captureId, ct)
            ?? throw new KeyNotFoundException($"Finish capture {captureId} not found.");
        var previousStatus = capture.UploadStatus;
        await captureRepo.UpdateUploadStatusAsync(captureId, status, ct);
        await auditLog.LogAsync(capture.TimingSessionId, AuditActions.FinishCaptureUploadStatusUpdated,
            capture.FinishDeviceId,
            details: new
            {
                captureId,
                previousStatus = previousStatus.ToString(),
                newStatus = status.ToString()
            }, ct: ct);
    }
}
