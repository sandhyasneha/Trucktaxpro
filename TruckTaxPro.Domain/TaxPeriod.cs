namespace TruckTaxPro.Domain;

public class TaxPeriod
{
    public int Id { get; set; }
    public int TaxYear { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
