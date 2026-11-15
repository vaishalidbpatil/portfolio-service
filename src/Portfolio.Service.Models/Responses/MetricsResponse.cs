namespace Portfolio.Service.Models.Responses;

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
