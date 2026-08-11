using System.ComponentModel.DataAnnotations.Schema;

namespace TruckTaxPro.Domain;

public class DiscountCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PercentOff { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? FlatAmountOff { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
}
