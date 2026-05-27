using TraficoTiming.Database.Enums;

namespace TraficoTiming.Database.Entities;

public class TimingSession
{
    public Guid Id { get; set; }

    public Guid MeetId { get; set; }
    public Guid EventId { get; set; }

    public int HeatNumber { get; set; }
    public string? Round { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Created;

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }

    public decimal? SyncQualityScore { get; set; }

    public ICollection<TimingDevice> Devices { get; set; } = new List<TimingDevice>();
    public ICollection<ClockSyncRecord> ClockSyncRecords { get; set; } = new List<ClockSyncRecord>();
    public ICollection<RaceStartEvent> RaceStartEvents { get; set; } = new List<RaceStartEvent>();
    public ICollection<FinishCapture> FinishCaptures { get; set; } = new List<FinishCapture>();
    public ICollection<RawTimingResult> RawTimingResults { get; set; } = new List<RawTimingResult>();
    public ICollection<TimingAuditLog> AuditLogs { get; set; } = new List<TimingAuditLog>();
}
