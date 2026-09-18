namespace eggs_accounting_app.Models;

public class StockAdjustment
{
    public int Id { get; set; }

    public int InventoryBatchId { get; set; }

    public int QuantityEggs { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string? LoggedBy { get; set; }

    public DateTime Timestamp { get; set; }

    public string? Grade { get; set; }
}