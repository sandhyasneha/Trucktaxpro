using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class RegisterViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select whether you're a Tax Payer or Tax Preparer.")]
    public bool IsTaxPreparer { get; set; }

    [Required(ErrorMessage = "Please select self-employed status.")]
    public bool IsSelfEmployed { get; set; }
}