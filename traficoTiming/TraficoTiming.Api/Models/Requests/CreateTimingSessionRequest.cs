namespace TraficoTiming.Api.Models.Requests;

public record CreateTimingSessionRequest(
    Guid MeetId,
    Guid EventId,
    int HeatNumber,
    string? Round,
    Guid CreatedByUserId);
