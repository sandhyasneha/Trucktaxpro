namespace TruckTaxPro.Application.Mef;

/// <summary>
/// Everything the MeF transmitter needs to build the IRS submission for one filing.
/// This is the JSON contract handed off at IMefTransmissionService.SubmitAsync — built once,
/// right after IRS Payment method + Service Fee are both confirmed.
/// </summary>
public class FilingPackageDto
{
    public int BusinessTaxPeriodId { get; set; }
    public string ConfirmationNumber { get; set; } = string.Empty;

    public FilingBusinessDto Business { get; set; } = new();
    public FilingTaxPeriodDto TaxPeriod { get; set; } = new();

    public List<FilingTaxableVehicleDto> TaxableVehicles { get; set; } = new();
    public List<FilingSuspendedVehicleDto> SuspendedVehicles { get; set; } = new();
    public List<FilingCreditVehicleDto> CreditVehicles { get; set; } = new();
    public List<FilingPriorYearSoldSuspendedVehicleDto> PriorYearSoldSuspendedVehicles { get; set; } = new();

    public FilingIrsPaymentDto IrsPayment { get; set; } = new();

    public decimal TotalTaxAmount { get; set; }
    public decimal TotalCreditAmount { get; set; }
    public decimal BalanceDue { get; set; }
}

public class FilingBusinessDto
{
    public string BusinessName { get; set; } = string.Empty;
    public string Ein { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class FilingTaxPeriodDto
{
    public int TaxYearStart { get; set; }
    public int TaxYearEnd { get; set; }
    public DateTime FirstUsedMonth { get; set; }
    public bool IsFinalReturn { get; set; }
    public bool ConsentToDisclosure { get; set; }
}

public class FilingTaxableVehicleDto
{
    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string WeightCategory { get; set; } = string.Empty;
    public bool IsLogging { get; set; }
    public decimal TaxAmount { get; set; }
}

public class FilingSuspendedVehicleDto
{
    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public int MileageLimit { get; set; }
}

public class FilingCreditVehicleDto
{
    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string WeightCategory { get; set; } = string.Empty;
    public bool IsLogging { get; set; }
    public string Reason { get; set; } = string.Empty; // "Sold", "Stolen", "Destroyed"
    public DateTime EffectiveDate { get; set; }
    public string? BuyerName { get; set; }
    public DateTime FirstUsedMonthPriorYear { get; set; }
    public decimal PreviouslyReportedTax { get; set; }
    public decimal TaxAmountUsed { get; set; }
    public decimal CreditAmount { get; set; }
}

public class FilingPriorYearSoldSuspendedVehicleDto
{
    public int UnitNumber { get; set; }
    public string Vin { get; set; } = string.Empty;
    public int MileageLimit { get; set; }
    public DateTime DateSold { get; set; }
    public string BuyerName { get; set; } = string.Empty;
}

public class FilingIrsPaymentDto
{
    public string PaymentMethod { get; set; } = string.Empty; // "EFW", "EFTPS", "CreditCard"

    // Only populated for EFW — required by the IRS for direct debit instructions.
    // NOTE: this DTO carries real bank account/routing data — treat any endpoint or log
    // that touches it as sensitive, and never expose it outside the authenticated,
    // server-to-server handoff to the MeF transmitter.
    public string? AccountType { get; set; }
    public string? AccountNumber { get; set; }
    public string? RoutingNumber { get; set; }
    public string? PhoneNumber { get; set; }
}
