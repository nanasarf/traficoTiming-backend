using Microsoft.EntityFrameworkCore;
using TraficoTiming.Database;
using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;

namespace TraficoTiming.Repository.Implementations;

public class FinishCaptureRepository(TraficoTimingDbContext db) : IFinishCaptureRepository
{
    public async Task<FinishCapture> CreateAsync(FinishCapture capture, CancellationToken ct = default)
    {
        db.FinishCaptures.Add(capture);
        await db.SaveChangesAsync(ct);
        return capture;
    }

    public Task<FinishCapture?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.FinishCaptures.FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<List<FinishCapture>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.FinishCaptures
            .Where(f => f.TimingSessionId == sessionId)
            .OrderBy(f => f.RecordingStartedAtUtc)
            .ToListAsync(ct);

    public async Task<List<FinishCapture>> GetByDeviceIdAsync(Guid deviceId, CancellationToken ct = default) =>
        await db.FinishCaptures
            .Where(f => f.FinishDeviceId == deviceId)
            .OrderByDescending(f => f.RecordingStartedAtUtc)
            .ToListAsync(ct);

    public async Task<List<FinishCapture>> GetPendingUploadsAsync(CancellationToken ct = default) =>
        await db.FinishCaptures
            .Where(f => f.UploadStatus == UploadStatus.LocalOnly || f.UploadStatus == UploadStatus.Failed)
            .OrderBy(f => f.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task UpdateAsync(FinishCapture capture, CancellationToken ct = default)
    {
        db.FinishCaptures.Update(capture);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateUploadStatusAsync(Guid captureId, UploadStatus status, CancellationToken ct = default) =>
        await db.FinishCaptures
            .Where(f => f.Id == captureId)
            .ExecuteUpdateAsync(f => f.SetProperty(x => x.UploadStatus, status), ct);

    public async Task DeleteAsync(FinishCapture capture, CancellationToken ct = default)
    {
        db.FinishCaptures.Remove(capture);
        await db.SaveChangesAsync(ct);
    }
}
