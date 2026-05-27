namespace TraficoTiming.Database.Entities;

public class TimingAuditLog
{
    public Guid Id { get; set; }

    public Guid TimingSessionId { get; set; }
    public TimingSession TimingSession { get; set; } = null!;

    public Guid? DeviceId { get; set; }
    public Guid? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? DetailsJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
