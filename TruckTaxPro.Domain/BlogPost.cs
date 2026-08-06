namespace TruckTaxPro.Domain;

public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;       // URL-friendly, e.g. "form-2290-deadline-2026"
    public string Summary { get; set; } = string.Empty;     // short teaser for the listing page
    public string Content { get; set; } = string.Empty;     // full HTML body
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}