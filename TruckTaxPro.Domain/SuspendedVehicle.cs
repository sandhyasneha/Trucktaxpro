namespace TruckTaxPro.Domain;

public class SuspendedVehicle
{
    public int Id { get; set; }

    public int BusinessTaxPeriodId { get; set; }
    public BusinessTaxPeriod? BusinessTaxPeriod { get; set; }

    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;

    // 5000 or 7500 (agricultural)
    public int MileageLimit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
