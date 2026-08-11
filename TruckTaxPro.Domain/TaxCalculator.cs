namespace TruckTaxPro.Domain;

/// <summary>
/// Computes Form 2290 tax for a single taxable vehicle.
/// Vehicles first used in July of the tax period pay the full annual rate.
/// Vehicles first used in any later month pay a prorated amount: annual rate x (months
/// remaining in the period, from the first-used month through June inclusive) / 12,
/// rounded to the nearest cent. This is the same method the IRS uses to generate its
/// published partial-period tax tables in the Form 2290 instructions.
/// </summary>
public static class TaxCalculator
{
    public static decimal ComputeTaxableVehicleTax(string weightCategory, bool isLogging, DateTime firstUsedMonth, int taxYearStart)
    {
        var rate = IrsWeightCategoryRates.Find(weightCategory)
            ?? throw new ArgumentException($"Unknown weight category '{weightCategory}'.", nameof(weightCategory));

        var annualRate = isLogging ? rate.LoggingAnnualTax : rate.StandardAnnualTax;

        var monthsRemaining = MonthsRemainingInPeriod(firstUsedMonth, taxYearStart);

        if (monthsRemaining == 12)
        {
            return annualRate;
        }

        var prorated = annualRate * monthsRemaining / 12m;
        return Math.Round(prorated, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Tax period runs July (month 1 of the period) through June (month 12 of the period).
    /// Returns how many months remain from the first-used month through the following June, inclusive.
    /// </summary>
    public static int MonthsRemainingInPeriod(DateTime firstUsedMonth, int taxYearStart)
    {
        var periodStart = new DateTime(taxYearStart, 7, 1);
        var monthsSincePeriodStart = ((firstUsedMonth.Year - periodStart.Year) * 12) + (firstUsedMonth.Month - periodStart.Month);

        if (monthsSincePeriodStart < 0 || monthsSincePeriodStart > 11)
        {
            throw new ArgumentOutOfRangeException(nameof(firstUsedMonth), "First used month falls outside the given tax period.");
        }

        return 12 - monthsSincePeriodStart;
    }

    /// <summary>
    /// Computes the credit amounts for a vehicle sold, destroyed, or stolen during a prior tax period.
    /// Returns the tax originally paid for the full period, the portion actually "used" (in service
    /// through the effective/loss date), and the credit refund for the remaining unused months.
    /// </summary>
    public static (decimal OriginalTax, decimal TaxAmountUsed, decimal CreditAmount) ComputeCreditVehicleAmounts(
        string weightCategory, bool isLogging, DateTime firstUsedMonthPriorYear, int priorTaxYearStart, DateTime effectiveDate)
    {
        var originalTax = ComputeTaxableVehicleTax(weightCategory, isLogging, firstUsedMonthPriorYear, priorTaxYearStart);

        var rate = IrsWeightCategoryRates.Find(weightCategory)
            ?? throw new ArgumentException($"Unknown weight category '{weightCategory}'.", nameof(weightCategory));
        var annualRate = isLogging ? rate.LoggingAnnualTax : rate.StandardAnnualTax;

        var monthAfterEffective = new DateTime(effectiveDate.Year, effectiveDate.Month, 1).AddMonths(1);

        int monthsRemainingAfter;
        try
        {
            monthsRemainingAfter = MonthsRemainingInPeriod(monthAfterEffective, priorTaxYearStart);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Effective date was in June, the last month of the period - nothing remains to credit.
            monthsRemainingAfter = 0;
        }

        var creditAmount = monthsRemainingAfter <= 0
            ? 0m
            : monthsRemainingAfter >= 12
                ? annualRate
                : Math.Round(annualRate * monthsRemainingAfter / 12m, 2, MidpointRounding.AwayFromZero);

        var taxAmountUsed = originalTax - creditAmount;

        return (originalTax, taxAmountUsed, creditAmount);
    }
}
