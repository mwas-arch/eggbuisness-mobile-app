using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EggBusiness.Api.Models;

public class InventoryBatch
{
    public int Id { get; set; }

    [Required]
    public int SupplierId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Grade { get; set; } = string.Empty;

    public int EggsReceived { get; set; }

    public int EggsRemaining { get; set; }

    public int EggsPerCrate { get; set; } = 30;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPerCrate { get; set; }

    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

    public Supplier? Supplier { get; set; }

    public ICollection<StockAdjustment> StockAdjustments { get; set; }
        = new List<StockAdjustment>();

    public ICollection<SaleItem> SaleItems { get; set; }
        = new List<SaleItem>();

    [NotMapped]
    public int RemainingCrates => EggsRemaining / EggsPerCrate;

    [NotMapped]
    public int RemainingLooseEggs => EggsRemaining % EggsPerCrate;
}