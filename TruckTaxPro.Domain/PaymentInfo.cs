using System.ComponentModel.DataAnnotations.Schema;

namespace TruckTaxPro.Domain;

public class PaymentInfo
{
    public int Id { get; set; }

    public int BusinessTaxPeriodId { get; set; }
    public BusinessTaxPeriod? BusinessTaxPeriod { get; set; }

    // "EFW", "EFTPS", or "CreditCard"
    public string PaymentMethod { get; set; } = string.Empty;

    // Only populated when PaymentMethod == "EFW"
    public string? AccountType { get; set; }   // "Checking" or "Saving"
    public string? AccountNumber { get; set; }
    public string? PhoneNumber { get; set; }
    public string? RoutingNumber { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalTaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCreditAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceDue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
