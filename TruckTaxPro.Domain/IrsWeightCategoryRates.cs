namespace TruckTaxPro.Domain;

/// <summary>
/// Official IRS Form 2290 annual tax rates, per weight category, for the July 2026 - June 2027 revision.
/// Source: Form 2290 (Rev. July 2026), page 2, Tax Computation table.
/// Update this table each year when the IRS publishes a new revision.
/// </summary>
public static class IrsWeightCategoryRates
{
    public record CategoryRate(string Category, string WeightRangeLabel, decimal StandardAnnualTax, decimal LoggingAnnualTax);

    public static readonly IReadOnlyList<CategoryRate> Categories = new List<CategoryRate>
    {
        new("A", "Up to 55,000 lbs",       100.00m, 75.00m),
        new("B", "55,001 - 56,000 lbs",    122.00m, 91.50m),
        new("C", "56,001 - 57,000 lbs",    144.00m, 108.00m),
        new("D", "57,001 - 58,000 lbs",    166.00m, 124.50m),
        new("E", "58,001 - 59,000 lbs",    188.00m, 141.00m),
        new("F", "59,001 - 60,000 lbs",    210.00m, 157.50m),
        new("G", "60,001 - 61,000 lbs",    232.00m, 174.00m),
        new("H", "61,001 - 62,000 lbs",    254.00m, 190.50m),
        new("I", "62,001 - 63,000 lbs",    276.00m, 207.00m),
        new("J", "63,001 - 64,000 lbs",    298.00m, 223.50m),
        new("K", "64,001 - 65,000 lbs",    320.00m, 240.00m),
        new("L", "65,001 - 66,000 lbs",    342.00m, 256.50m),
        new("M", "66,001 - 67,000 lbs",    364.00m, 273.00m),
        new("N", "67,001 - 68,000 lbs",    386.00m, 289.50m),
        new("O", "68,001 - 69,000 lbs",    408.00m, 306.00m),
        new("P", "69,001 - 70,000 lbs",    430.00m, 322.50m),
        new("Q", "70,001 - 71,000 lbs",    452.00m, 339.00m),
        new("R", "71,001 - 72,000 lbs",    474.00m, 355.50m),
        new("S", "72,001 - 73,000 lbs",    496.00m, 372.00m),
        new("T", "73,001 - 74,000 lbs",    518.00m, 388.50m),
        new("U", "74,001 - 75,000 lbs",    540.00m, 405.00m),
        new("V", "Over 75,000 lbs",        550.00m, 412.50m),
    };

    public static CategoryRate? Find(string category) =>
        Categories.FirstOrDefault(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
}
