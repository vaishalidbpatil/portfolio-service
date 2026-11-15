using Npgsql;
using Portfolio.Service.Configuration;
using Portfolio.Service.Models;
using Portfolio.Service.ServiceInterfaces;

namespace Portfolio.Service.DataAccess;

public class IngestionRepository : IIngestionRepository
{
    private readonly string _connectionString;

    public IngestionRepository(DatabaseOptions opts) =>
        _connectionString = opts.ConnectionString;

    public async Task<IngestionJob> CreateJobAsync(string source, string sourceHash, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO ingestion_jobs (id, source, source_hash, status, records_read, records_loaded, records_rejected, started_at)
            VALUES ($1, $2, $3, 'RUNNING', 0, 0, 0, now())
            RETURNING id, source, source_hash, status, records_read, records_loaded, records_rejected, started_at, finished_at
            """;
        var id = Guid.NewGuid();
        cmd.Parameters.AddWithValue(id);
        cmd.Parameters.AddWithValue(source);
        cmd.Parameters.AddWithValue(sourceHash);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return MapJob(reader);
    }

    public async Task UpdateJobAsync(IngestionJob job, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE ingestion_jobs
            SET status = $1, records_read = $2, records_loaded = $3, records_rejected = $4, finished_at = $5
            WHERE id = $6
            """;
        cmd.Parameters.AddWithValue(job.Status.ToString());
        cmd.Parameters.AddWithValue(job.RecordsRead);
        cmd.Parameters.AddWithValue(job.RecordsLoaded);
        cmd.Parameters.AddWithValue(job.RecordsRejected);
        cmd.Parameters.AddWithValue((object?)job.FinishedAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue(job.Id);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IngestionJob?> GetJobAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, source, source_hash, status, records_read, records_loaded, records_rejected, started_at, finished_at
            FROM ingestion_jobs WHERE id = $1
            """;
        cmd.Parameters.AddWithValue(id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapJob(reader) : null;
    }

    public async Task<IEnumerable<IngestionJob>> ListJobsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, source, source_hash, status, records_read, records_loaded, records_rejected, started_at, finished_at
            FROM ingestion_jobs
            ORDER BY started_at DESC
            LIMIT $1 OFFSET $2
            """;
        cmd.Parameters.AddWithValue(pageSize);
        cmd.Parameters.AddWithValue((page - 1) * pageSize);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var jobs = new List<IngestionJob>();
        while (await reader.ReadAsync(ct)) jobs.Add(MapJob(reader));
        return jobs;
    }

    private static IngestionJob MapJob(NpgsqlDataReader r) => new()
    {
        Id              = r.GetGuid(0),
        Source          = r.GetString(1),
        SourceHash      = r.GetString(2),
        Status          = Enum.Parse<IngestionStatus>(r.GetString(3)),
        RecordsRead     = r.GetInt32(4),
        RecordsLoaded   = r.GetInt32(5),
        RecordsRejected = r.GetInt32(6),
        StartedAt       = r.GetDateTime(7),
        FinishedAt      = r.IsDBNull(8) ? null : r.GetDateTime(8)
    };
}
