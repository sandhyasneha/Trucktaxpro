using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Security.Claims;
using Trucktaxpro.Models;
using TruckTaxPro.Data;
using TruckTaxPro.Domain;

namespace Trucktaxpro.Controllers;

[Authorize]
public class FilingController : Controller
{
    private readonly TruckTaxProDbContext _db;

    public FilingController(TruckTaxProDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult SelectBusiness()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var businesses = _db.Businesses
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.BusinessName)
            .Select(b => new BusinessSummaryViewModel { Id = b.Id, BusinessName = b.BusinessName })
            .ToList();

        return View(new BusinessListViewModel { Businesses = businesses });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult StartFiling(int businessId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var owns = _db.Businesses.Any(b => b.Id == businessId && b.UserId == userId);
        if (!owns)
        {
            return Forbid();
        }

        return RedirectToAction("TaxPeriod", new { businessId });
    }

    [HttpGet]
    public IActionResult Business(int? id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Business? existing = id.HasValue
            ? _db.Businesses.FirstOrDefault(b => b.Id == id.Value && b.UserId == userId)
            : null;

        if (id.HasValue && existing == null)
        {
            return NotFound();
        }

        var model = existing == null
            ? new BusinessViewModel()
            : new BusinessViewModel
            {
                Id = existing.Id,
                BusinessName = existing.BusinessName,
                Ein = existing.Ein,
                BusinessType = existing.BusinessType,
                AddressLine1 = existing.AddressLine1,
                City = existing.City,
                State = existing.State,
                ZipCode = existing.ZipCode,
                PhoneNumber = existing.PhoneNumber
            };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Business(BusinessViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Business business;

        if (model.Id > 0)
        {
            business = _db.Businesses.FirstOrDefault(b => b.Id == model.Id && b.UserId == userId)!;
            if (business == null)
            {
                return NotFound();
            }
        }
        else
        {
            business = new Business { UserId = userId! };
            _db.Businesses.Add(business);
        }

        business.BusinessName = model.BusinessName;
        business.Ein = model.Ein;
        business.BusinessType = model.BusinessType;
        business.AddressLine1 = model.AddressLine1;
        business.City = model.City;
        business.State = model.State;
        business.ZipCode = model.ZipCode;
        business.PhoneNumber = model.PhoneNumber;

        _db.SaveChanges();

        TempData["BusinessSaved"] = true;
        return RedirectToAction("TaxPeriod", new { businessId = business.Id });
    }

    [HttpGet]
    public IActionResult TaxPeriod(int businessId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var business = _db.Businesses.FirstOrDefault(b => b.Id == businessId && b.UserId == userId);
        if (business == null)
        {
            return NotFound();
        }

        var existing = _db.BusinessTaxPeriods
            .Where(t => t.BusinessId == businessId && t.Status == "Draft")
            .OrderByDescending(t => t.UpdatedAt)
            .FirstOrDefault();

        var model = existing == null
            ? new TaxPeriodViewModel { BusinessId = businessId, TaxYearStart = GetCurrentIrsTaxYearStart() }
            : new TaxPeriodViewModel
            {
                Id = existing.Id,
                BusinessId = businessId,
                TaxYearStart = existing.TaxYearStart,
                FirstUsedMonth = existing.FirstUsedMonth.ToString("yyyy-MM"),
                IsFinalReturn = existing.IsFinalReturn,
                ConsentToDisclosure = existing.ConsentToDisclosure
            };

        ViewBag.BusinessName = business.BusinessName;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TaxPeriod(TaxPeriodViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var business = _db.Businesses.FirstOrDefault(b => b.Id == model.BusinessId && b.UserId == userId);
        if (business == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var firstUsed = DateTime.ParseExact(model.FirstUsedMonth + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var periodStart = new DateTime(model.TaxYearStart, 7, 1);
            var periodEnd = new DateTime(model.TaxYearStart + 1, 6, 30);

            if (firstUsed < periodStart || firstUsed > periodEnd)
            {
                ModelState.AddModelError(nameof(model.FirstUsedMonth),
                    "First Used Month must fall within the selected tax period (July\u2013June).");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.BusinessName = business.BusinessName;
            return View(model);
        }

        var firstUsedMonth = DateTime.ParseExact(model.FirstUsedMonth + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);

        BusinessTaxPeriod taxPeriod;
        if (model.Id > 0)
        {
            taxPeriod = _db.BusinessTaxPeriods.FirstOrDefault(t => t.Id == model.Id && t.BusinessId == model.BusinessId)!;
        }
        else
        {
            taxPeriod = new BusinessTaxPeriod { BusinessId = model.BusinessId };
            _db.BusinessTaxPeriods.Add(taxPeriod);
        }

        taxPeriod.TaxYearStart = model.TaxYearStart;
        taxPeriod.TaxYearEnd = model.TaxYearStart + 1;
        taxPeriod.FirstUsedMonth = firstUsedMonth;
        taxPeriod.IsFinalReturn = model.IsFinalReturn;
        taxPeriod.ConsentToDisclosure = model.ConsentToDisclosure;
        taxPeriod.Status = "Draft";
        taxPeriod.CurrentStep = 3;
        taxPeriod.UpdatedAt = DateTime.UtcNow;

        _db.SaveChanges();

        // TODO: next build pass — Vehicles (step 3) replaces this direct hop.
        return RedirectToAction("Vehicles", new { businessId = model.BusinessId });
    }

    private static int GetCurrentIrsTaxYearStart()
    {
        var now = DateTime.UtcNow;
        return now.Month >= 7 ? now.Year : now.Year - 1;
    }

    [HttpGet]
    public IActionResult Vehicles(int businessId)
    {
        ViewBag.WeightCategories = GetWeightCategories();

        var priorYearVehicles = _db.Vehicles
            .Where(v => v.BusinessId == businessId && v.TaxYear == GetLastFiledYear(businessId))
            .ToList();

        var model = new VehicleListViewModel
        {
            BusinessId = businessId,
            Vehicles = priorYearVehicles.Select(v => new VehicleInputViewModel
            {
                Id = v.Id,
                BusinessId = businessId,
                Vin = v.Vin,
                WeightCategory = v.WeightCategory,
                IsAgricultural = v.IsAgricultural,
                IsSuspended = v.IsSuspended,
                IsFromPriorYear = true,
                IsConfirmed = false
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveVehicles(VehicleListViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.WeightCategories = GetWeightCategories();
            return View("Vehicles", model);
        }

        var rowsToSave = model.Vehicles.Where(v => !v.IsFromPriorYear || v.IsConfirmed).ToList();

        foreach (var row in rowsToSave)
        {
            if (row.Id > 0)
            {
                var existing = _db.Vehicles.Find(row.Id);
                if (existing != null)
                {
                    existing.Vin = row.Vin;
                    existing.WeightCategory = row.WeightCategory;
                    existing.IsAgricultural = row.IsAgricultural;
                    existing.IsSuspended = row.IsSuspended;
                    continue;
                }
            }

            _db.Vehicles.Add(new Vehicle
            {
                BusinessId = model.BusinessId,
                Vin = row.Vin,
                WeightCategory = row.WeightCategory,
                IsAgricultural = row.IsAgricultural,
                IsSuspended = row.IsSuspended,
                TaxYear = DateTime.UtcNow.Year
            });
        }

        _db.SaveChanges();

        TempData["Success"] = "Vehicles saved.";
        return RedirectToAction("TaxSummary", new { businessId = model.BusinessId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UploadVehicles(VehicleUploadViewModel upload, [FromForm] List<string> existingVins)
    {
        if (!ModelState.IsValid || upload.File == null || upload.File.Length == 0)
        {
            return Json(new { success = false, message = "Please choose a valid file." });
        }

        List<VehicleInputViewModel> parsedRows;

        try
        {
            var ext = Path.GetExtension(upload.File.FileName).ToLowerInvariant();
            parsedRows = ext switch
            {
                ".txt" => ParseTxt(upload.File, upload.BusinessId),
                ".xlsx" => ParseExcel(upload.File, upload.BusinessId),
                _ => throw new InvalidOperationException("Unsupported file type. Please upload a .txt or .xlsx file.")
            };
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }

        if (parsedRows.Count <= 5)
        {
            return Json(new
            {
                success = false,
                message = $"This file has {parsedRows.Count} VIN(s). Bulk upload is for fleets of 6 or more vehicles — please enter 5 or fewer VINs manually using the Add row below."
            });
        }

        var existingSet = new HashSet<string>((existingVins ?? new List<string>()).Select(v => v.ToUpperInvariant()));
        var seenInFile = new HashSet<string>();
        var results = new List<object>();

        foreach (var row in parsedRows)
        {
            var isDuplicate = existingSet.Contains(row.Vin) || !seenInFile.Add(row.Vin);

            results.Add(new
            {
                row.Vin,
                row.WeightCategory,
                row.IsAgricultural,
                row.IsSuspended,
                isDuplicate
            });
        }

        var duplicateCount = results.Count(r => (bool)r.GetType().GetProperty("isDuplicate")!.GetValue(r)!);

        return Json(new
        {
            success = true,
            rows = results,
            duplicateCount,
            message = duplicateCount > 0
                ? $"{duplicateCount} VIN(s) in this file already appear in your table — flagged below for your review."
                : null
        });
    }

    private List<VehicleInputViewModel> ParseTxt(IFormFile file, int businessId)
    {
        var rows = new List<VehicleInputViewModel>();
        using var reader = new StreamReader(file.OpenReadStream());
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var vin = line.Trim();
            if (string.IsNullOrWhiteSpace(vin)) continue;

            rows.Add(new VehicleInputViewModel
            {
                BusinessId = businessId,
                Vin = vin.ToUpperInvariant(),
                WeightCategory = string.Empty,
                IsAgricultural = false,
                IsSuspended = false
            });
        }
        return rows;
    }

    private List<VehicleInputViewModel> ParseExcel(IFormFile file, int businessId)
    {
        var rows = new List<VehicleInputViewModel>();
        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        for (int r = 2; r <= rowCount; r++)
        {
            var vin = worksheet.Cell(r, 1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(vin)) continue;

            rows.Add(new VehicleInputViewModel
            {
                BusinessId = businessId,
                Vin = vin.ToUpperInvariant(),
                WeightCategory = worksheet.Cell(r, 2).GetString().Trim(),
                IsAgricultural = ParseBool(worksheet.Cell(r, 3).GetString()),
                IsSuspended = ParseBool(worksheet.Cell(r, 4).GetString())
            });
        }
        return rows;
    }

    private bool ParseBool(string value) =>
        value.Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase) || value.Trim() == "1";

    private int GetLastFiledYear(int businessId)
    {
        var years = _db.Vehicles
            .Where(v => v.BusinessId == businessId)
            .Select(v => v.TaxYear)
            .ToList();

        return years.Count > 0 ? years.Max() : 0;
    }

    private List<SelectListItem> GetWeightCategories()
    {
        return new List<SelectListItem>
        {
            new() { Value = "A", Text = "A - 55,000 lbs" },
            new() { Value = "B", Text = "B - 55,001–56,000 lbs" },
            new() { Value = "C", Text = "C - 56,001–57,000 lbs" },
            new() { Value = "D", Text = "D - 57,001–58,000 lbs" },
            new() { Value = "E", Text = "E - 58,001–59,000 lbs" },
            new() { Value = "F", Text = "F - 59,001–60,000 lbs" },
            new() { Value = "G", Text = "G - 60,001–61,000 lbs" },
            new() { Value = "H", Text = "H - 61,001–62,000 lbs" },
            new() { Value = "I", Text = "I - 62,001–63,000 lbs" },
            new() { Value = "J", Text = "J - 63,001–64,000 lbs" },
            new() { Value = "K", Text = "K - 64,001–65,000 lbs" },
            new() { Value = "L", Text = "L - 65,001–66,000 lbs" },
            new() { Value = "M", Text = "M - 66,001–67,000 lbs" },
            new() { Value = "N", Text = "N - 67,001–68,000 lbs" },
            new() { Value = "O", Text = "O - 68,001–69,000 lbs" },
            new() { Value = "P", Text = "P - 69,001–70,000 lbs" },
            new() { Value = "Q", Text = "Q - 70,001–71,000 lbs" },
            new() { Value = "R", Text = "R - 71,001–72,000 lbs" },
            new() { Value = "S", Text = "S - 72,001–73,000 lbs" },
            new() { Value = "T", Text = "T - 73,001–74,000 lbs" },
            new() { Value = "U", Text = "U - 74,001–75,000 lbs" },
            new() { Value = "V", Text = "V - 75,001 lbs and over" },
            new() { Value = "W", Text = "W - Suspended (mileage exempt)" }
        };
    }
}
