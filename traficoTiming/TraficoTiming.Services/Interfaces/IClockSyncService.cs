using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Interfaces;

public interface IClockSyncService
{
    Task<ClockSyncRecord> SyncClocksAsync(ClockSyncModel model, CancellationToken ct = default);
    Task<ClockSyncRecord?> GetLatestSyncAsync(Guid sessionId, CancellationToken ct = default);
}
