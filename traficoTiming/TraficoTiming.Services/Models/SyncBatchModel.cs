namespace TraficoTiming.Services.Models;

public class SyncBatchModel
{
    public Guid TimingSessionId { get; set; }
    public Guid DeviceId { get; set; }
    public List<SyncItemModel> Items { get; set; } = [];
}

public class SyncItemModel
{
    public string ClientGeneratedId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}
