using TraficoTiming.Database.Enums;

namespace TraficoTiming.Database.Entities;

public class SyncBatch
{
    public Guid Id { get; set; }

    public Guid TimingSessionId { get; set; }
    public Guid DeviceId { get; set; }

    public SyncStatus Status { get; set; } = SyncStatus.Pending;

    public int ItemCount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }

    public string? ErrorMessage { get; set; }

    public ICollection<SyncItem> SyncItems { get; set; } = new List<SyncItem>();
}
