namespace TraficoTiming.Api.Models.Responses;

public class TimingAuditLogResponse
{
    public Guid Id { get; set; }
    public Guid TimingSessionId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? DeviceId { get; set; }
    public Guid? UserId { get; set; }
    public string? DetailsJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
