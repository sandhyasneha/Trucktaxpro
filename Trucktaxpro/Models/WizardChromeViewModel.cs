namespace Trucktaxpro.Models;

public class WizardChromeViewModel
{
    public List<string> Steps { get; set; } = new();
    public int CurrentStep { get; set; }

    // Used as a fallback greeting when the signed-in user has no FullName set.
    public string? ActiveBusinessName { get; set; }
}
