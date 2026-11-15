namespace Portfolio.Service.Models;

public class IngestionJob
{
    public Guid Id { get; init; }
    public string Source { get; init; } = "";
    public string SourceHash { get; init; } = "";
    public IngestionStatus Status { get; set; }
    public int RecordsRead { get; set; }
    public int RecordsLoaded { get; set; }
    public int RecordsRejected { get; set; }
    public DateTime StartedAt { get; init; }
    public DateTime? FinishedAt { get; set; }
}

public enum IngestionStatus { RUNNING, COMPLETED, FAILED }
