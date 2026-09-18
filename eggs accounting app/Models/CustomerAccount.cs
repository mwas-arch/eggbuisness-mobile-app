namespace eggs_accounting_app.Models;

public class CustomerAccount
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public decimal CreditLimit { get; set; }

    public decimal TotalPurchases { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal OutstandingBalance { get; set; }

    public decimal AvailableCredit { get; set; }

    public List<CustomerSale> Sales { get; set; } = new();
}

public class CustomerSale
{
    public int Id { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal BalanceDue { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }
}