using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class ResetPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}