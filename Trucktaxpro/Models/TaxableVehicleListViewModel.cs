namespace Trucktaxpro.Models;

public class TaxableVehicleListViewModel
{
    public int BusinessTaxPeriodId { get; set; }
    public List<TaxableVehicleRowViewModel> Vehicles { get; set; } = new();
    public TaxableVehicleInputViewModel NewVehicle { get; set; } = new();
    public decimal Total => Vehicles.Sum(v => v.TaxAmount);
}

public class TaxableVehicleRowViewModel
{
    public int Id { get; set; }
    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string WeightCategory { get; set; } = string.Empty;
    public string WeightCategoryLabel { get; set; } = string.Empty;
    public bool IsLogging { get; set; }
    public decimal TaxAmount { get; set; }
}
