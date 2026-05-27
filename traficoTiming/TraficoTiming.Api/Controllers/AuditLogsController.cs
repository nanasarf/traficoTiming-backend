using Microsoft.AspNetCore.Mvc;
using TraficoTiming.Api.Models.Responses;
using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Interfaces;

namespace TraficoTiming.Api.Controllers;

[ApiController]
[Route("api/timing")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet("sessions/{sessionId:guid}/audit-logs")]
    public async Task<IActionResult> GetSessionAuditLogs(Guid sessionId, CancellationToken cancellationToken)
    {
        var logs = await _auditLogService.GetLogsAsync(sessionId, cancellationToken);

        var responses = logs.Select(ToResponse).ToList();

        return Ok(ApiResponse<List<TimingAuditLogResponse>>.Ok(responses));
    }

    private static TimingAuditLogResponse ToResponse(TimingAuditLog log)
    {
        return new TimingAuditLogResponse
        {
            Id = log.Id,
            TimingSessionId = log.TimingSessionId,
            Action = log.Action,
            DeviceId = log.DeviceId,
            UserId = log.UserId,
            DetailsJson = log.DetailsJson,
            CreatedAtUtc = log.CreatedAtUtc
        };
    }
}
