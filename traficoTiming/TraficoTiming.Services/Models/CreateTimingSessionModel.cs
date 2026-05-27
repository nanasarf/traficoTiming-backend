namespace TraficoTiming.Services.Models;

public class CreateTimingSessionModel
{
    public Guid MeetId { get; set; }
    public Guid EventId { get; set; }
    public int HeatNumber { get; set; }
    public int Round { get; set; }
    public Guid CreatedByUserId { get; set; }
}
