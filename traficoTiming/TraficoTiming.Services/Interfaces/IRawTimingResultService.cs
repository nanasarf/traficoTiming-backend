using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Interfaces;

public interface IRawTimingResultService
{
    Task<RawTimingResult> CreateResultAsync(CreateRawTimingResultModel model, CancellationToken ct = default);
    Task<RawTimingResult> GetResultAsync(Guid resultId, CancellationToken ct = default);
    Task<IReadOnlyList<RawTimingResult>> GetSessionResultsAsync(Guid sessionId, CancellationToken ct = default);
    Task<RawTimingResult> UpdateResultAsync(UpdateRawTimingResultModel model, CancellationToken ct = default);
    Task<RawTimingResult> SubmitResultAsync(Guid resultId, CancellationToken ct = default);
    Task<RawTimingResult> RejectResultAsync(Guid resultId, string note, CancellationToken ct = default);
}
