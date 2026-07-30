using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class BusinessViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Business name is required.")]
    [StringLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [Required(ErrorMessage = "EIN is required.")]
    [RegularExpression(@"^\d{2}-\d{7}$", ErrorMessage = "EIN must be in the format XX-XXXXXXX.")]
    public string Ein { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a business type.")]
    public string BusinessType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(200)]
    public string AddressLine1 { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a state.")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "ZIP code is required.")]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "ZIP code must be 5 digits (optionally XXXXX-XXXX).")]
    public string ZipCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\(\d{3}\) \d{3}-\d{4}$", ErrorMessage = "Phone number must be in the format (XXX) XXX-XXXX.")]
    public string PhoneNumber { get; set; } = string.Empty;
}