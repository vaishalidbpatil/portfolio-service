using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Service.ServiceInterfaces;

namespace Portfolio.Service.Api.Controllers;

[ApiController]
[Route("v1/ingestion")]
[Authorize]
public class IngestionController : ControllerBase
{
    private readonly IIngestionService _ingestion;
    public IngestionController(IIngestionService ingestion) => _ingestion = ingestion;

    [HttpPost("jobs")]
    [EnableRateLimiting("orders-write")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> StartJob(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "A non-empty file is required." });

        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        if (ext is not ("csv" or "json"))
            return BadRequest(new { error = "Unsupported file type. Accepted: csv, json." });

        await using var stream = file.OpenReadStream();
        var job = await _ingestion.StartJobAsync(stream, file.FileName, ct);
        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
    }

    [HttpGet("jobs/{id:guid}")]
    [EnableRateLimiting("orders-read")]
    public async Task<IActionResult> GetJob(Guid id, CancellationToken ct)
    {
        var job = await _ingestion.GetJobAsync(id, ct);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpGet("jobs")]
    [EnableRateLimiting("orders-read")]
    public async Task<IActionResult> ListJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;
        var jobs = await _ingestion.ListJobsAsync(page, pageSize, ct);
        return Ok(jobs);
    }
}
