using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EggBusiness.Api.Models;

public class ProductPrice
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RetailPricePerEgg { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal WholesalePricePerCrate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BulkPricePerCrate { get; set; }

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
}