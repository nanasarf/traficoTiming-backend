using TraficoTiming.Database.Enums;

namespace TraficoTiming.Api.Models.Responses;

public class SyncBatchResponse
{
    public Guid Id { get; set; }
    public Guid TimingSessionId { get; set; }
    public Guid DeviceId { get; set; }
    public SyncStatus Status { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public string? ErrorMessage { get; set; }
}
