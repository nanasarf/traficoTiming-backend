namespace TraficoTiming.Api.Models.Requests;

public record CreateTimingSessionRequest(
    Guid MeetId,
    Guid EventId,
    int HeatNumber,
    int Round,
    Guid CreatedByUserId);
