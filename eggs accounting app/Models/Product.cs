using System.ComponentModel.DataAnnotations;

namespace eggs_accounting_app.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "Eggs";

    [Required]
    [MaxLength(50)]
    public string Grade { get; set; } = string.Empty;

    public int EggsPerCrate { get; set; } = 30;

    public ICollection<ProductPrice> Prices { get; set; }
        = new List<ProductPrice>();

    public ICollection<SaleItem> SaleItems { get; set; }
        = new List<SaleItem>();
}