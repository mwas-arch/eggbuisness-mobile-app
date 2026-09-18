namespace eggs_accounting_app.Models;

public class CreateSaleRequest
{
    public int? CustomerId { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal AmountPaid { get; set; }

    public string? CreatedBy { get; set; }

    public List<CreateSaleItemRequest> Items { get; set; } = new();
}

public class CreateSaleItemRequest
{
    public int ProductId { get; set; }

    public int QuantityEggs { get; set; }

    public string PriceType { get; set; } = "Retail";
}