using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class PriorYearSoldSuspendedVehicleInputViewModel
{
    public int Id { get; set; }

    [Required]
    public int BusinessTaxPeriodId { get; set; }

    [Required(ErrorMessage = "VIN is required.")]
    [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN must be exactly 17 characters.")]
    [RegularExpression(@"^[A-HJ-NPR-Za-hj-npr-z0-9]{17}$", ErrorMessage = "VIN must be 17 characters and cannot contain the letters I, O, or Q.")]
    public string Vin { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a mileage limit.")]
    [Range(5000, 7500, ErrorMessage = "Mileage limit must be 5,000 or 7,500 miles.")]
    public int MileageLimit { get; set; }

    [Required(ErrorMessage = "Date sold is required.")]
    public DateTime? DateSold { get; set; }

    [Required(ErrorMessage = "Buyer name is required.")]
    [StringLength(200)]
    public string BuyerName { get; set; } = string.Empty;
}
