namespace Portfolio.Service.Models;

public record IngestionRecord(
    string AccountId,
    string Symbol,
    string Side,
    int Quantity,
    decimal Price
);
