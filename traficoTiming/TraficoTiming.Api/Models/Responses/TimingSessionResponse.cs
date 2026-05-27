using TraficoTiming.Database.Enums;

namespace TraficoTiming.Api.Models.Responses;

public class TimingSessionResponse
{
    public Guid Id { get; set; }
    public Guid MeetId { get; set; }
    public Guid EventId { get; set; }
    public int HeatNumber { get; set; }
    public string? Round { get; set; }
    public SessionStatus Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public decimal? SyncQualityScore { get; set; }
}
