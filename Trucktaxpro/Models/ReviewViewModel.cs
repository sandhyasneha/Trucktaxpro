namespace Trucktaxpro.Models;

public class ReviewViewModel
{
    public int BusinessTaxPeriodId { get; set; }

    public string BusinessName { get; set; } = string.Empty;
    public string Ein { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

    public int TaxYearStart { get; set; }
    public int TaxYearEnd { get; set; }
    public DateTime FirstUsedMonth { get; set; }

    public int TaxableCount { get; set; }
    public int SuspendedCount { get; set; }
    public int CreditCount { get; set; }
    public int PriorYearSoldSuspendedCount { get; set; }

    public string IrsPaymentMethod { get; set; } = string.Empty;
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalCreditAmount { get; set; }
    public decimal BalanceDue { get; set; }

    public decimal ServiceFeeAmount { get; set; }
    public decimal OtherCharges { get; set; }
    public string StripePublishableKey { get; set; } = string.Empty;

    public string DefaultFirstName { get; set; } = string.Empty;
    public string DefaultAddressLine1 { get; set; } = string.Empty;
    public string DefaultCity { get; set; } = string.Empty;
    public string DefaultState { get; set; } = string.Empty;
    public string DefaultZip { get; set; } = string.Empty;
    public string DefaultPhone { get; set; } = string.Empty;
}

