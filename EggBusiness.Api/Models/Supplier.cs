using System.ComponentModel.DataAnnotations;

namespace EggBusiness.Api.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<InventoryBatch> InventoryBatches { get; set; }
        = new List<InventoryBatch>();
}