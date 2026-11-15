using Portfolio.Service.Models;

namespace Portfolio.Service.ServiceInterfaces;

public interface IIngestionRepository
{
    Task<IngestionJob> CreateJobAsync(string source, string sourceHash, CancellationToken ct = default);
    Task UpdateJobAsync(IngestionJob job, CancellationToken ct = default);
    Task<IngestionJob?> GetJobAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<IngestionJob>> ListJobsAsync(int page, int pageSize, CancellationToken ct = default);
}
