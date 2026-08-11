namespace Trucktaxpro.Models;

public class PriorYearSoldSuspendedVehicleListViewModel
{
    public int BusinessTaxPeriodId { get; set; }
    public List<PriorYearSoldSuspendedVehicleRowViewModel> Vehicles { get; set; } = new();
}

public class PriorYearSoldSuspendedVehicleRowViewModel
{
    public int Id { get; set; }
    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public int MileageLimit { get; set; }
    public DateTime DateSold { get; set; }
    public string BuyerName { get; set; } = string.Empty;
}
