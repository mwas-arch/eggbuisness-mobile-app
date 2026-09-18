using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EggBusiness.Api.Models;

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    public int InventoryBatchId { get; set; }

    // Atomic inventory quantity
    public int QuantityEggs { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public Sale? Sale { get; set; }

    public Product? Product { get; set; }

    public InventoryBatch? InventoryBatch { get; set; }

    [NotMapped]
    public int Crates => QuantityEggs / 30;

    [NotMapped]
    public int LooseEggs => QuantityEggs % 30;
}