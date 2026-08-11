namespace TruckTaxPro.Domain;

public class BusinessTaxPeriod
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    public Business? Business { get; set; }

    // IRS HVUT tax year runs July (TaxYearStart) through June (TaxYearStart + 1)
    public int TaxYearStart { get; set; }
    public int TaxYearEnd { get; set; }

    public DateTime FirstUsedMonth { get; set; }

    public bool IsFinalReturn { get; set; }
    public bool ConsentToDisclosure { get; set; }

    // Which vehicle categories apply to this filing - set on the Filing Category step.
    public bool IncludeTaxable { get; set; } = true;
    public bool IncludeSuspended { get; set; }
    public bool IncludeCredit { get; set; }
    public bool IncludePriorYearSoldSuspended { get; set; }

    // "Draft" while the wizard is in progress, "Submitted" once IRS payment is completed.
    public string Status { get; set; } = "Draft";

    // Resume pointer: which wizard step this filing was last on.
    // 1=Business, 2=TaxPeriod, 3=Vehicles, 4=IrsPayment, 5=Review, 6=Finish
    // (Filing Category and the individual vehicle-entry screens are all sub-screens of step 3.)
    public int CurrentStep { get; set; } = 2;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ADD these two properties into your EXISTING TruckTaxPro.Domain/BusinessTaxPeriod.cs —
    // this is NOT a standalone file, just the fields to paste into the class body.

    public string IrsSubmissionStatus { get; set; } = "Processing"; // "Processing", "Accepted", "Rejected"
    public string? IrsConfirmationNumber { get; set; }

}
