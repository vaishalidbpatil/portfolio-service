using Portfolio.Service.Models;
using Portfolio.Service.Models.Requests;
using Portfolio.Service.Models.Responses;

namespace Portfolio.Service.ServiceInterfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, string idempotencyKey);
    Task<OrderResponse?> GetOrderAsync(Guid id);
    Task<PagedResponse<OrderResponse>> ListOrdersAsync(string? accountId, string? symbol, OrderStatus? status, int page, int pageSize);
}
