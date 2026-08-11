namespace Trucktaxpro.Options;

public class PricingOptions
{
    public List<VehicleTierPrice> Filing2290Tiers { get; set; } = new();
    public FlatPrice Amendment { get; set; } = new();
    public FlatPrice VinCorrection { get; set; } = new();
}

public class VehicleTierPrice
{
    public int MaxVehicles { get; set; }   // upper bound of this bracket
    public decimal Amount { get; set; }
    public string StripePriceId { get; set; } = string.Empty;
}

public class FlatPrice
{
    public decimal Amount { get; set; }
    public string StripePriceId { get; set; } = string.Empty;
}
