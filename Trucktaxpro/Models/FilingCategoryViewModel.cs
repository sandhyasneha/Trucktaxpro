using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class FilingCategoryViewModel : IValidatableObject
{
    [Required]
    public int BusinessTaxPeriodId { get; set; }

    public bool IncludeTaxable { get; set; }
    public bool IncludeSuspended { get; set; }
    public bool IncludeCredit { get; set; }
    public bool IncludePriorYearSoldSuspended { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IncludeTaxable && !IncludeSuspended && !IncludeCredit && !IncludePriorYearSoldSuspended)
        {
            yield return new ValidationResult(
                "Select at least one vehicle category to continue.",
                new[] { nameof(IncludeTaxable) });
        }
    }
}
