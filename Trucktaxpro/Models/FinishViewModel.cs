namespace Trucktaxpro.Models;

public class FinishViewModel
{
    public int BusinessTaxPeriodId { get; set; }
    public string ConfirmationNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Processing"; // "Processing", "Accepted", "Rejected"
    public string? Schedule1Url { get; set; }
}
