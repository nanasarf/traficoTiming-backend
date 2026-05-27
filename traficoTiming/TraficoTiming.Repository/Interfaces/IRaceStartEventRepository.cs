using TraficoTiming.Database.Entities;

namespace TraficoTiming.Repository.Interfaces;

public interface IRaceStartEventRepository
{
    Task<RaceStartEvent> CreateAsync(RaceStartEvent raceStartEvent, CancellationToken ct = default);
    Task<RaceStartEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<RaceStartEvent>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<RaceStartEvent?> GetLatestBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<bool> ExistsForSessionAsync(Guid sessionId, CancellationToken ct = default);
}
