namespace eggs_accounting_app.Models;

public class ProductPrice
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public decimal RetailPricePerEgg { get; set; }

    public decimal WholesalePricePerCrate { get; set; }

    public decimal BulkPricePerCrate { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public string Grade { get; set; } = string.Empty;
}