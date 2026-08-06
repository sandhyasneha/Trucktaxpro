using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class TaxPeriodViewModel
{
    public int Id { get; set; }

    [Required]
    public int BusinessId { get; set; }

    [Required(ErrorMessage = "Please select the tax period.")]
    public int TaxYearStart { get; set; }

    // Bound from an <input type="month"> field, which submits "yyyy-MM".
    // Kept as a string to avoid unreliable DateTime model-binding of that format.
    [Required(ErrorMessage = "Please select the first used month.")]
    [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$", ErrorMessage = "Please select a valid month.")]
    public string FirstUsedMonth { get; set; } = string.Empty;

    public bool IsFinalReturn { get; set; }
    public bool ConsentToDisclosure { get; set; }
}
