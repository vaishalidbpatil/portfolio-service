namespace Portfolio.Service.Configuration;

public class CorrelationContext
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
}
