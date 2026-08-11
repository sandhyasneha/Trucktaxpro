using System.ComponentModel.DataAnnotations.Schema;

namespace TruckTaxPro.Domain;

public class TaxableVehicle
{
    public int Id { get; set; }

    public int BusinessTaxPeriodId { get; set; }
    public BusinessTaxPeriod? BusinessTaxPeriod { get; set; }

    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string WeightCategory { get; set; } = string.Empty;
    public bool IsLogging { get; set; }

    // Computed at save time via TaxCalculator, stored so it doesn't need recomputing on every read.
    [Column(TypeName = "decimal(10,2)")]
    public decimal TaxAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
