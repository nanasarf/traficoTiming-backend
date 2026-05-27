using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;

namespace TraficoTiming.Repository.Interfaces;

public interface ITimingSessionRepository
{
    Task<TimingSession> CreateAsync(TimingSession session, CancellationToken ct = default);
    Task<TimingSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TimingSession?> GetFullSessionAsync(Guid id, CancellationToken ct = default);
    Task<List<TimingSession>> GetByMeetIdAsync(Guid meetId, CancellationToken ct = default);
    Task<List<TimingSession>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default);
    Task<List<TimingSession>> GetRunningSessionsAsync(CancellationToken ct = default);
    Task<List<TimingSession>> GetCompletedSessionsAsync(CancellationToken ct = default);
    Task<TimingSession?> GetSessionWithDevicesAsync(Guid id, CancellationToken ct = default);
    Task<TimingSession?> GetSessionWithResultsAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(TimingSession session, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid sessionId, SessionStatus status, CancellationToken ct = default);
    Task DeleteAsync(TimingSession session, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
