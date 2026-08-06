namespace TruckTaxPro.Domain;

public class Vehicle
{
    public int Id { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string WeightCategory { get; set; } = string.Empty;
    public bool IsAgricultural { get; set; }
    public bool IsSuspended { get; set; }
    public int TaxYear { get; set; }

    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
}