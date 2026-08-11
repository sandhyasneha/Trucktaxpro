namespace Trucktaxpro.Models;

public class IrsPaymentViewModel
{
    public int BusinessTaxPeriodId { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalCreditAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public PaymentInputViewModel Payment { get; set; } = new();
}
