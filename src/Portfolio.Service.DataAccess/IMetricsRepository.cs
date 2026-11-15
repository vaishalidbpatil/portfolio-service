using Portfolio.Service.Models.Responses;

namespace Portfolio.Service.DataAccess;

public interface IMetricsRepository
{
    Task<MetricsResponse> GetMetricsAsync();
}
