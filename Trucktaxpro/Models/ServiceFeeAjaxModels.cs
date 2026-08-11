namespace Trucktaxpro.Models;

public class ApplyDiscountCodeRequest
{
    public int BusinessTaxPeriodId { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class CreateServiceFeeIntentRequest
{
    public int BusinessTaxPeriodId { get; set; }
    public string? DiscountCode { get; set; }
}

public class ConfirmServiceFeeRequest
{
    public int BusinessTaxPeriodId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
    public string? DiscountCode { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
