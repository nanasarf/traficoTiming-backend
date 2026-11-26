using Microsoft.AspNetCore.Mvc;
using TraficoTiming.Services.Replay;

namespace TraficoTiming.Api.Controllers;

[ApiController]
[Route("api/timing/replays")]
public sealed class HeatReplayController : ControllerBase
{
    // Replay is a read-only calculation. It never creates or publishes an official result.
    [HttpPost("evaluate")]
    public ActionResult<ReviewedLane[]> Evaluate(Heat heat)
    {
        try { return Ok(HeatReplay.Evaluate(heat)); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
    }
}
