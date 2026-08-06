using Microsoft.AspNetCore.Identity;

namespace TruckTaxPro.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsTaxPreparer { get; set; }
    public bool IsSelfEmployed { get; set; }
}