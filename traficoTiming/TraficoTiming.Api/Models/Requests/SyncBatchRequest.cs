namespace TraficoTiming.Api.Models.Requests;

public record SyncBatchRequest(
    Guid TimingSessionId,
    Guid DeviceId,
    List<SyncItemRequest> Items);

public record SyncItemRequest(
    Guid ClientGeneratedId,
    string EntityType,
    string PayloadJson);
