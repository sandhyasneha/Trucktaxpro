namespace TruckTaxPro.Domain;

public class Announcement
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
