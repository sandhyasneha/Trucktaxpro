using System.ComponentModel.DataAnnotations.Schema;

namespace TruckTaxPro.Domain;

public class CreditVehicle
{
    public int Id { get; set; }

    public int BusinessTaxPeriodId { get; set; }
    public BusinessTaxPeriod? BusinessTaxPeriod { get; set; }

    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string WeightCategory { get; set; } = string.Empty;
    public bool IsLogging { get; set; }

    // Sold, Stolen, or Destroyed
    public string Reason { get; set; } = string.Empty;

    public DateTime EffectiveDate { get; set; }

    // Only populated when Reason = Sold
    public string? BuyerName { get; set; }

    public DateTime FirstUsedMonthPriorYear { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PreviouslyReportedTax { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TaxAmountUsed { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal CreditAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
