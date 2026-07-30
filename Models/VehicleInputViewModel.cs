using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class VehicleInputViewModel : IValidatableObject
{
    public int Id { get; set; } // 0 = new row, >0 = existing DB record from a prior filing

    public int BusinessId { get; set; }

    [Required(ErrorMessage = "VIN is required.")]
    [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN must be exactly 17 characters.")]
    [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$", ErrorMessage = "VIN must be 17 characters and cannot contain the letters I, O, or Q.")]
    public string Vin { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a weight category.")]
    public string WeightCategory { get; set; } = string.Empty;

    public bool IsAgricultural { get; set; }
    public bool IsSuspended { get; set; }

    public bool IsFromPriorYear { get; set; }   // true if pre-loaded from last year's filing
    public bool IsConfirmed { get; set; }        // true once user clicks "Add" to carry it forward this year

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsAgricultural && IsSuspended)
        {
            yield return new ValidationResult(
                "A vehicle cannot be both Agricultural and Suspended — please select only one.",
                new[] { nameof(IsAgricultural), nameof(IsSuspended) });
        }
    }
}