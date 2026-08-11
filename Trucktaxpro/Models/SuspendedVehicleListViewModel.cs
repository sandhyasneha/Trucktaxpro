namespace Trucktaxpro.Models;

public class SuspendedVehicleListViewModel
{
    public int BusinessTaxPeriodId { get; set; }
    public List<SuspendedVehicleRowViewModel> Vehicles { get; set; } = new();
}

public class SuspendedVehicleRowViewModel
{
    public int Id { get; set; }
    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public int MileageLimit { get; set; }
}
