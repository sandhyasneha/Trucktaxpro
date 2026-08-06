using Microsoft.AspNetCore.Mvc;
using TruckTaxPro.Data;

namespace Trucktaxpro.Controllers;

public class BlogController : Controller
{
    private readonly TruckTaxProDbContext _db;

    public BlogController(TruckTaxProDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var posts = _db.BlogPosts
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedAt)
            .ToList();

        return View(posts);
    }

    [HttpGet("/__blogpost/{slug}")]
    public IActionResult Post(string slug)
    {
        var post = _db.BlogPosts.FirstOrDefault(p => p.Slug == slug && p.IsPublished);
        if (post == null)
        {
            return NotFound();
        }
        return View(post);
    }
}