using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;

namespace TraficoTiming.Repository.Interfaces;

public interface IRawTimingResultRepository
{
    Task<RawTimingResult> CreateRawResultAsync(RawTimingResult result, CancellationToken ct = default);
    Task<RawTimingResult?> GetRawResultByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<RawTimingResult>> GetResultsBySessionAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<RawTimingResult>> GetResultsByLaneAsync(Guid sessionId, int lane, CancellationToken ct = default);
    Task<List<RawTimingResult>> GetPendingResultsAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<RawTimingResult>> GetSubmittedResultsAsync(Guid sessionId, CancellationToken ct = default);
    Task<List<RawTimingResult>> GetResultsByStatusAsync(Guid sessionId, RawResultStatus status, CancellationToken ct = default);
    Task UpdateRawResultAsync(RawTimingResult result, CancellationToken ct = default);
    Task DeleteAsync(RawTimingResult result, CancellationToken ct = default);
    Task<bool> LaneExistsInSessionAsync(Guid sessionId, int lane, CancellationToken ct = default);
}
