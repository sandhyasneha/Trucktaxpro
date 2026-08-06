namespace Trucktaxpro.Models;

public class BusinessListViewModel
{
    public List<BusinessSummaryViewModel> Businesses { get; set; } = new();
}

public class BusinessSummaryViewModel
{
    public int Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
}
