namespace eggs_accounting_app.Models;

public class Sale
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public string CustomerName { get; set; } = "Walk-in Customer";

    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal BalanceDue { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }

    public string? CreatedBy { get; set; }

    public List<SaleItem> Items { get; set; } = new();
}