using Portfolio.Service.Models;
using Portfolio.Service.ServiceInterfaces;

namespace Portfolio.Service.Services.Ingestion;

/// <summary>
/// Pass-through checker — all records pass until the Python library is wired in (#18).
/// </summary>
public class NoOpDataQualityChecker : IDataQualityChecker
{
    public Task<(bool Passed, string? Reason)> CheckAsync(IngestionRecord record, CancellationToken ct = default)
        => Task.FromResult((true, (string?)null));
}
