using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class PaymentInputViewModel
{
    [Required]
    public int BusinessTaxPeriodId { get; set; }

    [Required(ErrorMessage = "Please select a payment method.")]
    public string PaymentMethod { get; set; } = string.Empty; // "EFW", "EFTPS", "CreditCard"

    public string? AccountType { get; set; }

    [RegularExpression(@"^\d{5,17}$", ErrorMessage = "Enter a valid account number (5-17 digits).")]
    public string? AccountNumber { get; set; }

    [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit phone number.")]
    public string? PhoneNumber { get; set; }

    [RegularExpression(@"^\d{9}$", ErrorMessage = "Routing number must be 9 digits.")]
    public string? RoutingNumber { get; set; }
}
