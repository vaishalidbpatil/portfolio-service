using Npgsql;
using Portfolio.Service.Configuration;
using Portfolio.Service.Models.Responses;

namespace Portfolio.Service.DataAccess;

public class MetricsRepository : IMetricsRepository
{
    private readonly DatabaseOptions _options;

    public MetricsRepository(DatabaseOptions options)
    {
        _options = options;
    }

    public async Task<MetricsResponse> GetMetricsAsync()
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("""
            SELECT
              COUNT(*)                                                          AS total,
              COUNT(*) FILTER (WHERE status = 'PENDING')                       AS pending,
              COUNT(*) FILTER (WHERE status = 'FILLED')                        AS filled,
              COUNT(*) FILTER (WHERE status = 'REJECTED')                      AS rejected,
              COUNT(*) FILTER (WHERE status = 'DEAD_LETTERED')                 AS dead_lettered,
              COUNT(*) FILTER (WHERE created_at > NOW() - INTERVAL '1 hour')   AS last_hour
            FROM orders;

            SELECT
              COUNT(*) FILTER (WHERE status = 'PENDING')  AS outbox_pending,
              COUNT(*) FILTER (WHERE status = 'SENT')     AS outbox_sent,
              COUNT(*) FILTER (WHERE status = 'FAILED')   AS outbox_failed
            FROM outbox;
            """, conn);

        await using var reader = await cmd.ExecuteReaderAsync();

        await reader.ReadAsync();
        var total       = reader.GetInt32(0);
        var pending     = reader.GetInt32(1);
        var filled      = reader.GetInt32(2);
        var rejected    = reader.GetInt32(3);
        var deadLettered = reader.GetInt32(4);
        var lastHour    = reader.GetInt32(5);

        await reader.NextResultAsync();
        await reader.ReadAsync();
        var outboxPending = reader.GetInt32(0);
        var outboxSent    = reader.GetInt32(1);
        var outboxFailed  = reader.GetInt32(2);

        return new MetricsResponse(
            total, pending, filled, rejected, deadLettered,
            lastHour, outboxPending, outboxSent, outboxFailed
        );
    }
}
