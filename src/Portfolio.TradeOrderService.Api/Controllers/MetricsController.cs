using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetMetrics()
    {
        var result = await _metrics.GetMetricsAsync();
        return Ok(result);
    }
}
