namespace TruckTaxPro.Application.Mef;

/// <summary>
/// The seam between TruckTaxPro's wizard and whatever handles actual IRS MeF transmission.
/// The MeF developer implements this interface (mapping FilingPackageDto into the required
/// IRS MeF XML/JSON schema and submitting it — to ATS first, then production), and swaps it
/// in via one line in Program.cs. Nothing in FilingController needs to change when that happens.
/// </summary>
public interface IMefTransmissionService
{
    Task<MefTransmissionResult> SubmitAsync(FilingPackageDto package, CancellationToken cancellationToken = default);
}

public class MefTransmissionResult
{
    public bool Success { get; set; }
    public string Status { get; set; } = "Processing"; // "Processing", "Accepted", "Rejected"
    public string? IrsSubmissionId { get; set; }
    public string? ErrorMessage { get; set; }
}

