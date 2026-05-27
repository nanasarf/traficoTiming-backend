using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;

namespace TraficoTiming.Repository.Interfaces;

public interface IFinishCaptureRepository
{
    Task<FinishCapture> CreateAsync(FinishCapture capture, CancellationToken ct = default);
    Task<FinishCapture?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<FinishCapture>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<FinishCapture>> GetByDeviceIdAsync(Guid deviceId, CancellationToken ct = default);
    Task<List<FinishCapture>> GetPendingUploadsAsync(CancellationToken ct = default);
    Task UpdateAsync(FinishCapture capture, CancellationToken ct = default);
    Task UpdateUploadStatusAsync(Guid captureId, UploadStatus status, CancellationToken ct = default);
    Task DeleteAsync(FinishCapture capture, CancellationToken ct = default);
}
