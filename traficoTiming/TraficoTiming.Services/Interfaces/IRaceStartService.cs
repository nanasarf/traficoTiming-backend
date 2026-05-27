using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Interfaces;

public interface IRaceStartService
{
    Task<RaceStartEvent> StartRaceAsync(StartRaceModel model, CancellationToken ct = default);
    Task<RaceStartEvent?> GetRaceStartAsync(Guid sessionId, CancellationToken ct = default);
}
