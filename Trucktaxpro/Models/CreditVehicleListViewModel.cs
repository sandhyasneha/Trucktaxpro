namespace Trucktaxpro.Models;

public class CreditVehicleListViewModel
{
    public int BusinessTaxPeriodId { get; set; }
    public List<CreditVehicleRowViewModel> Vehicles { get; set; } = new();
    public decimal TotalCredit => Vehicles.Sum(v => v.CreditAmount);
}

public class CreditVehicleRowViewModel
{
    public int Id { get; set; }
    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string WeightCategory { get; set; } = string.Empty;
    public bool IsLogging { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public decimal PreviouslyReportedTax { get; set; }
    public decimal TaxAmountUsed { get; set; }
    public decimal CreditAmount { get; set; }
}
