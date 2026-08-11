namespace TruckTaxPro.Application.Mef;

/// <summary>
/// PLACEHOLDER. No real IRS transmission happens here — it just waits, then reports "Accepted"
/// so the wizard's Finish screen has something real to poll against during development.
/// Delete this once IMefTransmissionService has a real implementation, and update the
/// registration in Program.cs to point at the real class instead.
/// </summary>
public class PlaceholderMefTransmissionService : IMefTransmissionService
{
    public async Task<MefTransmissionResult> SubmitAsync(FilingPackageDto package, CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(45), cancellationToken);

        return new MefTransmissionResult
        {
            Success = true,
            Status = "Accepted",
            IrsSubmissionId = $"SIM-{Guid.NewGuid():N}".Substring(0, 16).ToUpperInvariant()
        };
    }
}
