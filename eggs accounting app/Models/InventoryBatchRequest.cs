namespace eggs_accounting_app.Models;

public class CreateInventoryBatchRequest
{
    public int SupplierId { get; set; }

    public string Grade { get; set; } = string.Empty;

    public int EggsReceived { get; set; }

    public int EggsPerCrate { get; set; } = 30;

    public decimal CostPerCrate { get; set; }

    public DateTime ReceivedDate { get; set; }
}