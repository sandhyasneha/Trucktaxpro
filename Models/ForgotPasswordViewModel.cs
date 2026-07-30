using System.ComponentModel.DataAnnotations;

namespace Trucktaxpro.Models;

public class ForgotPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}