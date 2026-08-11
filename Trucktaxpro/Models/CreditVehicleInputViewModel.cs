using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class CreditVehicleInputViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    public int BusinessTaxPeriodId { get; set; }

    [Required(ErrorMessage = "VIN is required.")]
    [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN must be exactly 17 characters.")]
    [RegularExpression(@"^[A-HJ-NPR-Za-hj-npr-z0-9]{17}$", ErrorMessage = "VIN must be 17 characters and cannot contain the letters I, O, or Q.")]
    public string Vin { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a weight category.")]
    public string WeightCategory { get; set; } = string.Empty;

    public bool IsLogging { get; set; }

    [Required(ErrorMessage = "Please select a reason.")]
    public string Reason { get; set; } = string.Empty;

    [Required(ErrorMessage = "Effective date is required.")]
    public DateTime? EffectiveDate { get; set; }

    [StringLength(200)]
    public string? BuyerName { get; set; }

    [Required(ErrorMessage = "First used month (prior year) is required.")]
    [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$", ErrorMessage = "Please select a valid month.")]
    public string FirstUsedMonthPriorYear { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Reason == "Sold" && string.IsNullOrWhiteSpace(BuyerName))
        {
            yield return new ValidationResult(
                "Buyer name is required when the reason is Sold.",
                new[] { nameof(BuyerName) });
        }
    }
}

