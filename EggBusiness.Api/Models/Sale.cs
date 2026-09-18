using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EggBusiness.Api.Models;

public class Sale
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceDue { get; set; }

    [Required]
    [MaxLength(30)]
    public string PaymentMethod { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public Customer? Customer { get; set; }

    public ICollection<SaleItem> SaleItems { get; set; }
        = new List<SaleItem>();
}