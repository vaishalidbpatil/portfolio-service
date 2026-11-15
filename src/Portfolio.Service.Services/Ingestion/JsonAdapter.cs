using System.Runtime.CompilerServices;
using System.Text.Json;
using Portfolio.Service.Models;
using Portfolio.Service.ServiceInterfaces;

namespace Portfolio.Service.Services.Ingestion;

public class JsonAdapter : ISourceAdapter
{
    public string SourceType => "json";

    public async IAsyncEnumerable<IngestionRecord> ReadAsync(
        Stream source, [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var doc = await JsonDocument.ParseAsync(source, cancellationToken: ct);

        if (doc.RootElement.ValueKind != JsonValueKind.Array) yield break;

        foreach (var el in doc.RootElement.EnumerateArray())
        {
            var record = TryParse(el);
            if (record is not null) yield return record;
        }
    }

    private static IngestionRecord? TryParse(JsonElement el)
    {
        try
        {
            return new IngestionRecord(
                AccountId: el.GetProperty("accountId").GetString()!,
                Symbol:    el.GetProperty("symbol").GetString()!.ToUpperInvariant(),
                Side:      el.GetProperty("side").GetString()!.ToUpperInvariant(),
                Quantity:  el.GetProperty("quantity").GetInt32(),
                Price:     el.GetProperty("price").GetDecimal());
        }
        catch { return null; }
    }
}
