using System.ComponentModel.DataAnnotations.Schema;

namespace TruckTaxPro.Domain;

public class ServiceFeePayment
{
    public int Id { get; set; }

    public int BusinessTaxPeriodId { get; set; }
    public BusinessTaxPeriod? BusinessTaxPeriod { get; set; }

    // Stripe references only — NEVER store card number or CVV.
    public string StripeCustomerId { get; set; } = string.Empty;
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public string StripePaymentMethodId { get; set; } = string.Empty;

    // Safe-to-store display data, sourced from Stripe's response — never entered manually.
    public string CardBrand { get; set; } = string.Empty;
    public string Last4 { get; set; } = string.Empty;
    public int ExpMonth { get; set; }
    public int ExpYear { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ServiceFeeAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherCharges { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCharged { get; set; }

    public string? DiscountCode { get; set; }

    public string BillingName { get; set; } = string.Empty;
    public string BillingAddressLine1 { get; set; } = string.Empty;
    public string? BillingAddressLine2 { get; set; }
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingZip { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    // "Succeeded", "Failed"
    public string Status { get; set; } = "Succeeded";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}