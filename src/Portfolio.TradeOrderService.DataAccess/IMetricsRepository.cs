using Portfolio.TradeOrderService.Models.Responses;

namespace Portfolio.TradeOrderService.DataAccess;

public interface IMetricsRepository
{
    Task<MetricsResponse> GetMetricsAsync();
}
