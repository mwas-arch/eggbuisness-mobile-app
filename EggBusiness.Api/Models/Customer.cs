using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EggBusiness.Api.Models;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditLimit { get; set; }

    public ICollection<Sale> Sales { get; set; }
        = new List<Sale>();
}