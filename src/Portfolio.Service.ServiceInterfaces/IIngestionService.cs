using Portfolio.Service.Models;

namespace Portfolio.Service.ServiceInterfaces;

public interface IIngestionService
{
    Task<IngestionJob> StartJobAsync(Stream source, string fileName, CancellationToken ct = default);
    Task<IngestionJob?> GetJobAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<IngestionJob>> ListJobsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
}
