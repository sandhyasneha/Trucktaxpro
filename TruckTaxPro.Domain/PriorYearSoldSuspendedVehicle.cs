namespace TruckTaxPro.Domain;

/// <summary>
/// A vehicle that was already suspended (tax-exempt) when it was sold during the prior tax
/// period. No tax was ever paid on it, so there's nothing to calculate here - this is purely
/// the informational declaration Form 2290 Part II, line 9 requires.
/// </summary>
public class PriorYearSoldSuspendedVehicle
{
    public int Id { get; set; }

    public int BusinessTaxPeriodId { get; set; }
    public BusinessTaxPeriod? BusinessTaxPeriod { get; set; }

    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;

    // 5000 or 7500 (agricultural)
    public int MileageLimit { get; set; }

    public DateTime DateSold { get; set; }
    public string BuyerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
