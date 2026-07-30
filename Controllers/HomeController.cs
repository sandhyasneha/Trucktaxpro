using Microsoft.AspNetCore.Mvc;
using TruckTaxPro.Data;

namespace Trucktaxpro.Controllers;

public class HomeController : Controller
{
    private readonly TruckTaxProDbContext _db;

    public HomeController(TruckTaxProDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var announcement = _db.Announcements
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        ViewBag.Announcement = announcement?.Message;

        return View();
    }

    [Route("/Faq")]
    public IActionResult Faq()
    {
        return View();
    }

    [Route("/Amendment")]
    public IActionResult Amendment()
    {
        return View();
    }

    [Route("/VinCorrection")]
    public IActionResult VinCorrection()
    {
        return View();
    }

    [Route("/2290Filing")]
    public IActionResult Filing2290()
    {
        return View();
    }

    [Route("/PrivacyPolicy")]
    public IActionResult Privacy() => View();

    [Route("/TermsOfUse")]
    public IActionResult Terms() => View();

    [Route("/RefundPolicy")]
    public IActionResult Refund() => View();

    [Route("/Security")]
    public IActionResult Security() => View();
}