using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Interfaces;

public interface ITimingSessionService
{
    Task<TimingSession> CreateSessionAsync(CreateTimingSessionModel model, CancellationToken ct = default);
    Task<TimingSession> GetSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task<IReadOnlyList<TimingSession>> GetSessionsByMeetAsync(Guid meetId, CancellationToken ct = default);
    Task<TimingSession> EndSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task<TimingSession> CancelSessionAsync(Guid sessionId, CancellationToken ct = default);
}
