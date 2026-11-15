using Portfolio.Service.Models;

namespace Portfolio.Service.ServiceInterfaces;

public interface ISourceAdapter
{
    string SourceType { get; }
    IAsyncEnumerable<IngestionRecord> ReadAsync(Stream source, CancellationToken ct = default);
}
