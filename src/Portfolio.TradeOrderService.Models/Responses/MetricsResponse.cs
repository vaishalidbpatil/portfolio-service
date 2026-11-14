namespace Portfolio.TradeOrderService.Models.Responses;

public record MetricsResponse(
    int TotalOrders,
    int Pending,
    int Filled,
    int Rejected,
    int DeadLettered,
    int OrdersLastHour,
    int OutboxPending,
    int OutboxSent,
    int OutboxFailed
);
