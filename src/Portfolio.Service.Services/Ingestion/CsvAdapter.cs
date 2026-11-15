using Portfolio.Service.Models;
using Portfolio.Service.ServiceInterfaces;
using System.Runtime.CompilerServices;

namespace Portfolio.Service.Services.Ingestion;

public class CsvAdapter : ISourceAdapter
{
    public string SourceType => "csv";

    public async IAsyncEnumerable<IngestionRecord> ReadAsync(
        Stream source, [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var reader = new StreamReader(source, leaveOpen: true);

        var header = await reader.ReadLineAsync(ct);
        if (header is null) yield break;

        var cols = ParseCsvLine(header);
        int iAccount = IndexOf(cols, "accountid"),
            iSymbol  = IndexOf(cols, "symbol"),
            iSide    = IndexOf(cols, "side"),
            iQty     = IndexOf(cols, "quantity"),
            iPrice   = IndexOf(cols, "price");

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var fields = ParseCsvLine(line);
            if (fields.Length <= Math.Max(iAccount, Math.Max(iSymbol, Math.Max(iSide, Math.Max(iQty, iPrice)))))
                continue;

            if (!int.TryParse(fields[iQty], out var qty)) continue;
            if (!decimal.TryParse(fields[iPrice], out var price)) continue;

            yield return new IngestionRecord(
                AccountId: fields[iAccount].Trim(),
                Symbol:    fields[iSymbol].Trim().ToUpperInvariant(),
                Side:      fields[iSide].Trim().ToUpperInvariant(),
                Quantity:  qty,
                Price:     price);
        }
    }

    private static int IndexOf(string[] headers, string name) =>
        Array.FindIndex(headers, h => h.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));

    private static string[] ParseCsvLine(string line)
    {
        // Minimal RFC-4180: handles quoted fields with embedded commas
        var fields = new List<string>();
        var i = 0;
        while (i < line.Length)
        {
            if (line[i] == '"')
            {
                i++;
                var sb = new System.Text.StringBuilder();
                while (i < line.Length)
                {
                    if (line[i] == '"' && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i += 2; }
                    else if (line[i] == '"') { i++; break; }
                    else { sb.Append(line[i++]); }
                }
                fields.Add(sb.ToString());
                if (i < line.Length && line[i] == ',') i++;
            }
            else
            {
                var end = line.IndexOf(',', i);
                if (end == -1) { fields.Add(line[i..]); break; }
                fields.Add(line[i..end]);
                i = end + 1;
            }
        }
        return [.. fields];
    }
}
