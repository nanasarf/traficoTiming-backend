using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Interfaces;

public interface IFinishCaptureService
{
    Task<FinishCapture> CreateCaptureAsync(CreateFinishCaptureModel model, CancellationToken ct = default);
    Task<IReadOnlyList<FinishCapture>> GetCapturesAsync(Guid sessionId, CancellationToken ct = default);
    Task UpdateUploadStatusAsync(Guid captureId, UploadStatus status, CancellationToken ct = default);
}
