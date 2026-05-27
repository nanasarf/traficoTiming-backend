using TraficoTiming.Database.Enums;

namespace TraficoTiming.Database.Entities;

public class SyncItem
{
    public Guid Id { get; set; }

    public Guid SyncBatchId { get; set; }
    public SyncBatch SyncBatch { get; set; } = null!;

    public Guid ClientGeneratedId { get; set; }

    public string EntityType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;

    public SyncStatus Status { get; set; } = SyncStatus.Pending;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
}
