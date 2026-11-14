using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.TradeOrderService.DataAccess;

namespace Portfolio.TradeOrderService.Api.Controllers;

[ApiController]
[Route("v1/metrics")]
[AllowAnonymous]
public class MetricsController : ControllerBase
{
    private readonly IMetricsRepository _metrics;

    public MetricsController(IMetricsRepository metrics)
    {
        _metrics = metrics;
    }

    [HttpGet]
    [EnableRateLimiting("metrics-read")]
    public async Task<IActionResult> GetMetrics()
    {
        var result = await _metrics.GetMetricsAsync();
        return Ok(result);
    }
}
