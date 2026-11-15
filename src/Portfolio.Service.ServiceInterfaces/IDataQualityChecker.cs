using Portfolio.Service.Models;

namespace Portfolio.Service.ServiceInterfaces;

/// <summary>
/// Runs data-quality checks on a record before ingestion.
/// Full implementation tracked in issue #18 (Python data-quality library).
/// </summary>
public interface IDataQualityChecker
{
    Task<(bool Passed, string? Reason)> CheckAsync(IngestionRecord record, CancellationToken ct = default);
}
