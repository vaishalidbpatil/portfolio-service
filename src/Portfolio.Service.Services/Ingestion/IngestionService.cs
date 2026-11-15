using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Portfolio.Service.Models;
using Portfolio.Service.Models.Requests;
using Portfolio.Service.ServiceInterfaces;

namespace Portfolio.Service.Services.Ingestion;

public class IngestionService : IIngestionService
{
    private readonly IEnumerable<ISourceAdapter> _adapters;
    private readonly IIngestionRepository _repo;
    private readonly IOrderService _orders;
    private readonly IDataQualityChecker _dq;

    public IngestionService(
        IEnumerable<ISourceAdapter> adapters,
        IIngestionRepository repo,
        IOrderService orders,
        IDataQualityChecker dq)
    {
        _adapters = adapters;
        _repo = repo;
        _orders = orders;
        _dq = dq;
    }

    public async Task<IngestionJob> StartJobAsync(Stream source, string fileName, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        var adapter = _adapters.FirstOrDefault(a => a.SourceType == ext)
            ?? throw new NotSupportedException($"No adapter registered for file type '{ext}'. Supported: {string.Join(", ", _adapters.Select(a => a.SourceType))}");

        // Buffer the stream so we can hash the whole file and then read records
        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, ct);
        var sourceHash = ComputeHash(buffer.ToArray());

        var job = await _repo.CreateJobAsync(fileName, sourceHash, ct);

        try
        {
            buffer.Position = 0;
            await RunPipelineAsync(job, adapter, buffer, ct);
            job.Status = IngestionStatus.COMPLETED;
        }
        catch (Exception ex)
        {
            job.Status = IngestionStatus.FAILED;
            job.RecordsRejected += 1;
            _ = ex; // logged by caller
        }
        finally
        {
            job.FinishedAt = DateTime.UtcNow;
            await _repo.UpdateJobAsync(job, ct);
        }

        return job;
    }

    private async Task RunPipelineAsync(IngestionJob job, ISourceAdapter adapter, Stream source, CancellationToken ct)
    {
        await foreach (var record in adapter.ReadAsync(source, ct))
        {
            job.RecordsRead++;

            var (passed, reason) = await _dq.CheckAsync(record, ct);
            if (!passed) { job.RecordsRejected++; continue; }

            // Per-record idempotency key: SHA-256 of the normalised record content
            var idempotencyKey = ComputeRecordHash(record);

            try
            {
                var request = new CreateOrderRequest
                {
                    AccountId = record.AccountId,
                    Symbol = record.Symbol,
                    Side = Enum.Parse<OrderSide>(record.Side, ignoreCase: true),
                    Quantity = record.Quantity,
                    Price = record.Price
                };

                await _orders.CreateOrderAsync(request, idempotencyKey);
                job.RecordsLoaded++;
            }
            catch (Exception)
            {
                job.RecordsRejected++;
            }
        }
    }

    public Task<IngestionJob?> GetJobAsync(Guid id, CancellationToken ct = default)
        => _repo.GetJobAsync(id, ct);

    public Task<IEnumerable<IngestionJob>> ListJobsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
        => _repo.ListJobsAsync(page, pageSize, ct);

    private static string ComputeHash(byte[] data)
        => Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();

    private static string ComputeRecordHash(IngestionRecord r)
    {
        var json = JsonSerializer.Serialize(r);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
    }
}
