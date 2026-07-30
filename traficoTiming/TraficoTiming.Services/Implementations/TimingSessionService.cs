using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Implementations;

public class TimingSessionService(
    ITimingSessionRepository sessionRepo,
    IAuditLogService auditLog) : ITimingSessionService
{
    public async Task<TimingSession> CreateSessionAsync(CreateTimingSessionModel model, CancellationToken ct = default)
    {
        var session = new TimingSession
        {
            MeetId          = model.MeetId,
            EventId         = model.EventId,
            HeatNumber      = model.HeatNumber,
            Round           = model.Round,
            CreatedByUserId = model.CreatedByUserId,
            Status          = SessionStatus.Created
        };
        await sessionRepo.CreateAsync(session, ct);
        await auditLog.LogAsync(session.Id, AuditActions.SessionCreated, userId: session.CreatedByUserId,
            details: new { status = session.Status.ToString() }, ct: ct);
        return session;
    }

    public async Task<TimingSession> GetSessionAsync(Guid sessionId, CancellationToken ct = default) =>
        await sessionRepo.GetByIdAsync(sessionId, ct)
            ?? throw new KeyNotFoundException($"Timing session {sessionId} not found.");

    public async Task<IReadOnlyList<TimingSession>> GetSessionsByMeetAsync(Guid meetId, CancellationToken ct = default) =>
        (await sessionRepo.GetByMeetIdAsync(meetId, ct)).AsReadOnly();

    public async Task<TimingSession> EndSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await GetSessionAsync(sessionId, ct);
        if (session.Status is SessionStatus.Cancelled or SessionStatus.Completed)
            throw new InvalidOperationException($"Session is already {session.Status}.");

        var previousStatus = session.Status;
        session.Status     = SessionStatus.Completed;
        session.EndedAtUtc = DateTime.UtcNow;
        await sessionRepo.UpdateAsync(session, ct);
        await auditLog.LogAsync(session.Id, AuditActions.SessionCompleted, userId: session.CreatedByUserId,
            details: new
            {
                previousStatus = previousStatus.ToString(),
                newStatus = session.Status.ToString(),
                reason = "session completed"
            }, ct: ct);
        return session;
    }

    public async Task<TimingSession> CancelSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await GetSessionAsync(sessionId, ct);
        if (session.Status == SessionStatus.Completed)
            throw new InvalidOperationException("A completed session cannot be cancelled.");

        var previousStatus = session.Status;
        session.Status     = SessionStatus.Cancelled;
        session.EndedAtUtc = DateTime.UtcNow;
        await sessionRepo.UpdateAsync(session, ct);
        await auditLog.LogAsync(session.Id, AuditActions.SessionCancelled, userId: session.CreatedByUserId,
            details: new
            {
                previousStatus = previousStatus.ToString(),
                newStatus = session.Status.ToString(),
                reason = "session cancelled"
            }, ct: ct);
        return session;
    }
}
