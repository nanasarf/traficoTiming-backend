using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Implementations;

public class FinishCaptureService(
    IFinishCaptureRepository captureRepo,
    IAuditLogService auditLog) : IFinishCaptureService
{
    public async Task<FinishCapture> CreateCaptureAsync(CreateFinishCaptureModel model, CancellationToken ct = default)
    {
        var capture = new FinishCapture
        {
            TimingSessionId               = model.TimingSessionId,
            FinishDeviceId                = model.FinishDeviceId,
            LocalFileId                   = model.LocalFileId,
            FrameRate                     = (decimal)(model.FrameRate ?? 0),
            Resolution                    = model.Resolution ?? string.Empty,
            RecordingStartedAtUtc         = model.RecordingStartedAtUtc ?? DateTime.UtcNow,
            RecordingEndedAtUtc           = DateTime.UtcNow,
            FinishLineCalibrationDataJson = model.FinishLineCalibrationDataJson,
            UploadStatus                  = UploadStatus.LocalOnly
        };
        await captureRepo.CreateAsync(capture, ct);
        return capture;
    }

    public async Task<IReadOnlyList<FinishCapture>> GetCapturesAsync(Guid sessionId, CancellationToken ct = default) =>
        (await captureRepo.GetBySessionIdAsync(sessionId, ct)).AsReadOnly();

    public Task UpdateUploadStatusAsync(Guid captureId, UploadStatus status, CancellationToken ct = default) =>
        captureRepo.UpdateUploadStatusAsync(captureId, status, ct);
}
