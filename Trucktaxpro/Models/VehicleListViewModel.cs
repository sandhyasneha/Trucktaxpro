using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class VehicleListViewModel
{
    public int BusinessId { get; set; }

    public List<VehicleInputViewModel> Vehicles { get; set; } = new();
}