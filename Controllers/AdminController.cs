using Microsoft.AspNetCore.Mvc;
using TruckTaxPro.Data;
using TruckTaxPro.Domain;

namespace Trucktaxpro.Controllers;

public class AdminController : Controller
{
    private readonly TruckTaxProDbContext _db;
    private readonly IConfiguration _config;

    public AdminController(TruckTaxProDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    private bool IsAdminLoggedIn()
    {
        return HttpContext.Session.GetString("IsAdmin") == "true";
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (IsAdminLoggedIn())
        {
            return RedirectToAction("Dashboard");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password)
    {
        var adminEmail = _config["AdminAuth:Email"];
        var adminPassword = _config["AdminAuth:Password"];

        if (email == adminEmail && password == adminPassword)
        {
            HttpContext.Session.SetString("IsAdmin", "true");
            return RedirectToAction("Dashboard");
        }

        ModelState.AddModelError("", "Invalid email or password.");
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("IsAdmin");
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Dashboard()
    {
        if (!IsAdminLoggedIn())
        {
            return RedirectToAction("Login");
        }

        var currentAnnouncement = _db.Announcements
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        return View(currentAnnouncement);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PostAnnouncement(string message)
    {
        if (!IsAdminLoggedIn())
        {
            return RedirectToAction("Login");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            TempData["Error"] = "Announcement message cannot be empty.";
            return RedirectToAction("Dashboard");
        }

        var existing = _db.Announcements.Where(a => a.IsActive).ToList();
        foreach (var a in existing)
        {
            a.IsActive = false;
        }

        _db.Announcements.Add(new Announcement
        {
            Message = message.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        _db.SaveChanges();

        TempData["Success"] = "Announcement posted.";
        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveAnnouncement()
    {
        if (!IsAdminLoggedIn())
        {
            return RedirectToAction("Login");
        }

        var active = _db.Announcements.Where(a => a.IsActive).ToList();
        foreach (var a in active)
        {
            a.IsActive = false;
        }

        _db.SaveChanges();

        TempData["Success"] = "Announcement removed.";
        return RedirectToAction("Dashboard");
    }

    // ---------- Blog management ----------

    [HttpGet]
    public IActionResult BlogList()
    {
        if (!IsAdminLoggedIn())
        {
            return RedirectToAction("Login");
        }

        var posts = _db.BlogPosts.OrderByDescending(p => p.CreatedAt).ToList();
        return View(posts);
    }

    [HttpGet]
    public IActionResult BlogEdit(int? id)
    {
        if (!IsAdminLoggedIn())
        {
            return RedirectToAction("Login");
        }

        var post = id.HasValue ? _db.BlogPosts.Find(id.Value) : new BlogPost();
        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BlogEdit(BlogPost model)
    {
        if (!IsAdminLoggedIn())
        {
            return RedirectToAction("Login");
        }

        if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Content))
        {
            TempData["Error"] = "Title and Content are required.";
            return RedirectToAction("BlogEdit", new { id = model.Id });
        }

        if (string.IsNullOrWhiteSpace(model.Slug))
        {
            model.Slug = model.Title.ToLower()
                .Replace(" ", "-")
                .Where(c => char.IsLetterOrDigit(c) || c == '-')
                .Aggregate("", (s, c) => s + c);
        }

        if (model.Id > 0)
        {
            var existing = _db.BlogPosts.Find(model.Id);
            if (existing != null)
            {
                existing.Title = model.Title;
                existing.Slug = model.Slug;
                existing.Summary = model.Summary;
                existing.Content = model.Content;
                existing.IsPublished = model.IsPublished;
                existing.PublishedAt = model.IsPublished && existing.PublishedAt == null ? DateTime.UtcNow : existing.PublishedAt;
            }
        }
        else
        {
            model.PublishedAt = model.IsPublished ? DateTime.UtcNow : null;
            _db.BlogPosts.Add(model);
        }

        _db.SaveChanges();
        TempData["Success"] = "Blog post saved.";
        return RedirectToAction("BlogList");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BlogDelete(int id)
    {
        if (!IsAdminLoggedIn())
        {
            return RedirectToAction("Login");
        }

        var post = _db.BlogPosts.Find(id);
        if (post != null)
        {
            _db.BlogPosts.Remove(post);
            _db.SaveChanges();
        }

        TempData["Success"] = "Blog post deleted.";
        return RedirectToAction("BlogList");
    }
}
