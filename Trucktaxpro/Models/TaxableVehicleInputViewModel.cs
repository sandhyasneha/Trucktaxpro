using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class TaxableVehicleInputViewModel
{
    public int Id { get; set; } // 0 = new row

    [Required]
    public int BusinessTaxPeriodId { get; set; }

    [Required(ErrorMessage = "VIN is required.")]
    [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN must be exactly 17 characters.")]
    [RegularExpression(@"^[A-HJ-NPR-Za-hj-npr-z0-9]{17}$", ErrorMessage = "VIN must be 17 characters and cannot contain the letters I, O, or Q.")]
    public string Vin { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a weight category.")]
    public string WeightCategory { get; set; } = string.Empty;

    public bool IsLogging { get; set; }
}
