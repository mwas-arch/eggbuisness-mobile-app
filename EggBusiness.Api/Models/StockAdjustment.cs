using System.ComponentModel.DataAnnotations;

namespace EggBusiness.Api.Models;

public class StockAdjustment
{
    public int Id { get; set; }

    [Required]
    public int InventoryBatchId { get; set; }

    public int QuantityEggs { get; set; }

    [Required]
    [MaxLength(100)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? LoggedBy { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public InventoryBatch? InventoryBatch { get; set; }
}