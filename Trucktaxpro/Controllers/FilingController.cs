using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using System.Globalization;
using System.Security.Claims;
using Trucktaxpro.Models;
using Trucktaxpro.Options;
using TruckTaxPro.Application.Mef;
using TruckTaxPro.Data;
using TruckTaxPro.Domain;

namespace Trucktaxpro.Controllers;

[Authorize]
public class FilingController : Controller
{
    private readonly TruckTaxProDbContext _db;
    private readonly IConfiguration _config;
    private readonly PricingOptions _pricing;
    private readonly IServiceScopeFactory _scopeFactory;

    public FilingController(
        TruckTaxProDbContext db,
        IConfiguration config,
        IOptions<PricingOptions> pricing,
        IServiceScopeFactory scopeFactory)
    {
        _db = db;
        _config = config;
        _pricing = pricing.Value;
        _scopeFactory = scopeFactory;
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

        return RedirectToAction("FilingCategory", new { businessTaxPeriodId = taxPeriod.Id });
    }

    [HttpGet]
    public IActionResult FilingCategory(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        var model = new FilingCategoryViewModel
        {
            BusinessTaxPeriodId = taxPeriod.Id,
            IncludeTaxable = taxPeriod.IncludeTaxable,
            IncludeSuspended = taxPeriod.IncludeSuspended,
            IncludeCredit = taxPeriod.IncludeCredit,
            IncludePriorYearSoldSuspended = taxPeriod.IncludePriorYearSoldSuspended
        };

        ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
        ViewBag.BusinessId = taxPeriod.BusinessId;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FilingCategory(FilingCategoryViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == model.BusinessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
            ViewBag.BusinessId = taxPeriod.BusinessId;
            return View(model);
        }

        // If a category was previously included and is now unchecked, its vehicle rows would
        // otherwise be orphaned — still tied to this BusinessTaxPeriodId, still counted in
        // Review's summary and in the tax/credit totals, even though the user removed the
        // category. Clean those up here so the two always stay in sync.
        if (taxPeriod.IncludeTaxable && !model.IncludeTaxable)
        {
            _db.TaxableVehicles.RemoveRange(_db.TaxableVehicles.Where(v => v.BusinessTaxPeriodId == taxPeriod.Id));
        }
        if (taxPeriod.IncludeSuspended && !model.IncludeSuspended)
        {
            _db.SuspendedVehicles.RemoveRange(_db.SuspendedVehicles.Where(v => v.BusinessTaxPeriodId == taxPeriod.Id));
        }
        if (taxPeriod.IncludeCredit && !model.IncludeCredit)
        {
            _db.CreditVehicles.RemoveRange(_db.CreditVehicles.Where(v => v.BusinessTaxPeriodId == taxPeriod.Id));
        }
        if (taxPeriod.IncludePriorYearSoldSuspended && !model.IncludePriorYearSoldSuspended)
        {
            _db.PriorYearSoldSuspendedVehicles.RemoveRange(_db.PriorYearSoldSuspendedVehicles.Where(v => v.BusinessTaxPeriodId == taxPeriod.Id));
        }

        taxPeriod.IncludeTaxable = model.IncludeTaxable;
        taxPeriod.IncludeSuspended = model.IncludeSuspended;
        taxPeriod.IncludeCredit = model.IncludeCredit;
        taxPeriod.IncludePriorYearSoldSuspended = model.IncludePriorYearSoldSuspended;
        taxPeriod.CurrentStep = 3;
        taxPeriod.UpdatedAt = DateTime.UtcNow;

        _db.SaveChanges();

        return RouteToNextVehicleStep(taxPeriod, "start");
    }

    [HttpGet]
    public IActionResult TaxableVehicle(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
        ViewBag.WeightCategories = IrsWeightCategoryRates.Categories;
        ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "Taxable");

        var model = BuildTaxableVehicleListViewModel(businessTaxPeriodId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddTaxableVehicle(TaxableVehicleInputViewModel input)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == input.BusinessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            input.Vin = input.Vin.Trim().ToUpperInvariant();

            var duplicateVin = _db.TaxableVehicles.Any(v =>
                v.BusinessTaxPeriodId == input.BusinessTaxPeriodId &&
                v.Vin == input.Vin &&
                v.Id != input.Id);

            if (duplicateVin)
            {
                ModelState.AddModelError(nameof(input.Vin), "This VIN has already been added to this filing.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
            ViewBag.WeightCategories = IrsWeightCategoryRates.Categories;
            ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "Taxable");
            var errorModel = BuildTaxableVehicleListViewModel(input.BusinessTaxPeriodId);
            errorModel.NewVehicle = input;
            return View("TaxableVehicle", errorModel);
        }

        var taxAmount = TaxCalculator.ComputeTaxableVehicleTax(
            input.WeightCategory, input.IsLogging, taxPeriod.FirstUsedMonth, taxPeriod.TaxYearStart);

        if (input.Id > 0)
        {
            var existing = _db.TaxableVehicles.FirstOrDefault(v => v.Id == input.Id && v.BusinessTaxPeriodId == input.BusinessTaxPeriodId);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Vin = input.Vin;
            existing.WeightCategory = input.WeightCategory;
            existing.IsLogging = input.IsLogging;
            existing.TaxAmount = taxAmount;
        }
        else
        {
            var nextUnitNumber = _db.TaxableVehicles.Count(v => v.BusinessTaxPeriodId == input.BusinessTaxPeriodId) + 1;
            _db.TaxableVehicles.Add(new TaxableVehicle
            {
                BusinessTaxPeriodId = input.BusinessTaxPeriodId,
                UnitNumber = nextUnitNumber,
                Vin = input.Vin,
                WeightCategory = input.WeightCategory,
                IsLogging = input.IsLogging,
                TaxAmount = taxAmount
            });
        }

        _db.SaveChanges();

        return RedirectToAction("TaxableVehicle", new { businessTaxPeriodId = input.BusinessTaxPeriodId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteTaxableVehicle(int id, int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var owns = _db.BusinessTaxPeriods.Any(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);
        if (!owns)
        {
            return NotFound();
        }

        var vehicle = _db.TaxableVehicles.FirstOrDefault(v => v.Id == id && v.BusinessTaxPeriodId == businessTaxPeriodId);
        if (vehicle != null)
        {
            _db.TaxableVehicles.Remove(vehicle);
            _db.SaveChanges();

            var remaining = _db.TaxableVehicles
                .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
                .OrderBy(v => v.UnitNumber)
                .ToList();

            for (int i = 0; i < remaining.Count; i++)
            {
                remaining[i].UnitNumber = i + 1;
            }
            _db.SaveChanges();
        }

        return RedirectToAction("TaxableVehicle", new { businessTaxPeriodId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NextFromTaxableVehicle(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        var hasVehicles = _db.TaxableVehicles.Any(v => v.BusinessTaxPeriodId == businessTaxPeriodId);
        if (!hasVehicles)
        {
            TempData["TaxableVehicleError"] = "Add at least one taxable vehicle before continuing.";
            return RedirectToAction("TaxableVehicle", new { businessTaxPeriodId });
        }

        return RouteToNextVehicleStep(taxPeriod, "Taxable");
    }

    [HttpGet]
    public IActionResult SuspendedVehicle(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
        ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "Suspended");

        var model = BuildSuspendedVehicleListViewModel(businessTaxPeriodId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddSuspendedVehicle(SuspendedVehicleInputViewModel input)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == input.BusinessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            input.Vin = input.Vin.Trim().ToUpperInvariant();

            var duplicateVin = _db.SuspendedVehicles.Any(v =>
                v.BusinessTaxPeriodId == input.BusinessTaxPeriodId &&
                v.Vin == input.Vin &&
                v.Id != input.Id);

            if (duplicateVin)
            {
                ModelState.AddModelError(nameof(input.Vin), "This VIN has already been added to this filing.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
            ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "Suspended");
            var errorModel = BuildSuspendedVehicleListViewModel(input.BusinessTaxPeriodId);
            return View("SuspendedVehicle", errorModel);
        }

        if (input.Id > 0)
        {
            var existing = _db.SuspendedVehicles.FirstOrDefault(v => v.Id == input.Id && v.BusinessTaxPeriodId == input.BusinessTaxPeriodId);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Vin = input.Vin;
            existing.MileageLimit = input.MileageLimit;
        }
        else
        {
            var nextUnitNumber = _db.SuspendedVehicles.Count(v => v.BusinessTaxPeriodId == input.BusinessTaxPeriodId) + 1;
            _db.SuspendedVehicles.Add(new SuspendedVehicle
            {
                BusinessTaxPeriodId = input.BusinessTaxPeriodId,
                UnitNumber = nextUnitNumber,
                Vin = input.Vin,
                MileageLimit = input.MileageLimit
            });
        }

        _db.SaveChanges();

        return RedirectToAction("SuspendedVehicle", new { businessTaxPeriodId = input.BusinessTaxPeriodId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteSuspendedVehicle(int id, int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var owns = _db.BusinessTaxPeriods.Any(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);
        if (!owns)
        {
            return NotFound();
        }

        var vehicle = _db.SuspendedVehicles.FirstOrDefault(v => v.Id == id && v.BusinessTaxPeriodId == businessTaxPeriodId);
        if (vehicle != null)
        {
            _db.SuspendedVehicles.Remove(vehicle);
            _db.SaveChanges();

            var remaining = _db.SuspendedVehicles
                .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
                .OrderBy(v => v.UnitNumber)
                .ToList();

            for (int i = 0; i < remaining.Count; i++)
            {
                remaining[i].UnitNumber = i + 1;
            }
            _db.SaveChanges();
        }

        return RedirectToAction("SuspendedVehicle", new { businessTaxPeriodId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NextFromSuspendedVehicle(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        var hasVehicles = _db.SuspendedVehicles.Any(v => v.BusinessTaxPeriodId == businessTaxPeriodId);
        if (!hasVehicles)
        {
            TempData["SuspendedVehicleError"] = "Add at least one suspended vehicle before continuing.";
            return RedirectToAction("SuspendedVehicle", new { businessTaxPeriodId });
        }

        return RouteToNextVehicleStep(taxPeriod, "Suspended");
    }

    [HttpGet]
    public IActionResult CreditVehicle(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
        ViewBag.WeightCategories = IrsWeightCategoryRates.Categories;
        ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "Credit");

        var model = BuildCreditVehicleListViewModel(businessTaxPeriodId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddCreditVehicle(CreditVehicleInputViewModel input)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == input.BusinessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            input.Vin = input.Vin.Trim().ToUpperInvariant();

            var duplicateVin = _db.CreditVehicles.Any(v =>
                v.BusinessTaxPeriodId == input.BusinessTaxPeriodId &&
                v.Vin == input.Vin &&
                v.Id != input.Id);

            if (duplicateVin)
            {
                ModelState.AddModelError(nameof(input.Vin), "This VIN has already been added to this filing.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
            ViewBag.WeightCategories = IrsWeightCategoryRates.Categories;
            ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "Credit");
            var errorModel = BuildCreditVehicleListViewModel(input.BusinessTaxPeriodId);
            return View("CreditVehicle", errorModel);
        }

        var priorTaxYearStart = taxPeriod.TaxYearStart - 1;
        var priorPeriodStart = new DateTime(priorTaxYearStart, 7, 1);
        var priorPeriodEnd = new DateTime(priorTaxYearStart + 1, 6, 30);

        if (input.EffectiveDate!.Value.Date < priorPeriodStart || input.EffectiveDate.Value.Date > priorPeriodEnd)
        {
            ModelState.AddModelError(nameof(input.EffectiveDate),
                $"Effective date must fall within the prior tax period ({priorPeriodStart:MMM d, yyyy} - {priorPeriodEnd:MMM d, yyyy}), since credit vehicles were reported and taxed on last year's return.");
        }

        var firstUsedMonthPriorYear = DateTime.ParseExact(
            input.FirstUsedMonthPriorYear + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (firstUsedMonthPriorYear < priorPeriodStart || firstUsedMonthPriorYear > priorPeriodEnd)
        {
            ModelState.AddModelError(nameof(input.FirstUsedMonthPriorYear),
                $"First used month must fall within the prior tax period ({priorPeriodStart:MMM yyyy} - {priorPeriodEnd:MMM yyyy}).");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
            ViewBag.WeightCategories = IrsWeightCategoryRates.Categories;
            ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "Credit");
            var revalidateErrorModel = BuildCreditVehicleListViewModel(input.BusinessTaxPeriodId);
            return View("CreditVehicle", revalidateErrorModel);
        }

        var (originalTax, taxAmountUsed, creditAmount) = TaxCalculator.ComputeCreditVehicleAmounts(
            input.WeightCategory, input.IsLogging, firstUsedMonthPriorYear, priorTaxYearStart, input.EffectiveDate.Value);

        if (input.Id > 0)
        {
            var existing = _db.CreditVehicles.FirstOrDefault(v => v.Id == input.Id && v.BusinessTaxPeriodId == input.BusinessTaxPeriodId);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Vin = input.Vin;
            existing.WeightCategory = input.WeightCategory;
            existing.IsLogging = input.IsLogging;
            existing.Reason = input.Reason;
            existing.EffectiveDate = input.EffectiveDate.Value;
            existing.BuyerName = input.Reason == "Sold" ? input.BuyerName : null;
            existing.FirstUsedMonthPriorYear = firstUsedMonthPriorYear;
            existing.PreviouslyReportedTax = originalTax;
            existing.TaxAmountUsed = taxAmountUsed;
            existing.CreditAmount = creditAmount;
        }
        else
        {
            var nextUnitNumber = _db.CreditVehicles.Count(v => v.BusinessTaxPeriodId == input.BusinessTaxPeriodId) + 1;
            _db.CreditVehicles.Add(new CreditVehicle
            {
                BusinessTaxPeriodId = input.BusinessTaxPeriodId,
                UnitNumber = nextUnitNumber,
                Vin = input.Vin,
                WeightCategory = input.WeightCategory,
                IsLogging = input.IsLogging,
                Reason = input.Reason,
                EffectiveDate = input.EffectiveDate.Value,
                BuyerName = input.Reason == "Sold" ? input.BuyerName : null,
                FirstUsedMonthPriorYear = firstUsedMonthPriorYear,
                PreviouslyReportedTax = originalTax,
                TaxAmountUsed = taxAmountUsed,
                CreditAmount = creditAmount
            });
        }

        _db.SaveChanges();

        return RedirectToAction("CreditVehicle", new { businessTaxPeriodId = input.BusinessTaxPeriodId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteCreditVehicle(int id, int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var owns = _db.BusinessTaxPeriods.Any(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);
        if (!owns)
        {
            return NotFound();
        }

        var vehicle = _db.CreditVehicles.FirstOrDefault(v => v.Id == id && v.BusinessTaxPeriodId == businessTaxPeriodId);
        if (vehicle != null)
        {
            _db.CreditVehicles.Remove(vehicle);
            _db.SaveChanges();

            var remaining = _db.CreditVehicles
                .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
                .OrderBy(v => v.UnitNumber)
                .ToList();

            for (int i = 0; i < remaining.Count; i++)
            {
                remaining[i].UnitNumber = i + 1;
            }
            _db.SaveChanges();
        }

        return RedirectToAction("CreditVehicle", new { businessTaxPeriodId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NextFromCreditVehicle(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        var hasVehicles = _db.CreditVehicles.Any(v => v.BusinessTaxPeriodId == businessTaxPeriodId);
        if (!hasVehicles)
        {
            TempData["CreditVehicleError"] = "Add at least one credit vehicle before continuing.";
            return RedirectToAction("CreditVehicle", new { businessTaxPeriodId });
        }

        return RouteToNextVehicleStep(taxPeriod, "Credit");
    }

    [HttpGet]
    public IActionResult PriorYearSoldSuspendedVehicle(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
        ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "PriorYearSoldSuspended");

        var model = BuildPriorYearSoldSuspendedVehicleListViewModel(businessTaxPeriodId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddPriorYearSoldSuspendedVehicle(PriorYearSoldSuspendedVehicleInputViewModel input)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == input.BusinessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            input.Vin = input.Vin.Trim().ToUpperInvariant();

            var duplicateVin = _db.PriorYearSoldSuspendedVehicles.Any(v =>
                v.BusinessTaxPeriodId == input.BusinessTaxPeriodId &&
                v.Vin == input.Vin &&
                v.Id != input.Id);

            if (duplicateVin)
            {
                ModelState.AddModelError(nameof(input.Vin), "This VIN has already been added to this filing.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
            ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "PriorYearSoldSuspended");
            var errorModel = BuildPriorYearSoldSuspendedVehicleListViewModel(input.BusinessTaxPeriodId);
            return View("PriorYearSoldSuspendedVehicle", errorModel);
        }

        if (input.Id > 0)
        {
            var existing = _db.PriorYearSoldSuspendedVehicles.FirstOrDefault(v => v.Id == input.Id && v.BusinessTaxPeriodId == input.BusinessTaxPeriodId);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Vin = input.Vin;
            existing.MileageLimit = input.MileageLimit;
            existing.DateSold = input.DateSold!.Value;
            existing.BuyerName = input.BuyerName;
        }
        else
        {
            var nextUnitNumber = _db.PriorYearSoldSuspendedVehicles.Count(v => v.BusinessTaxPeriodId == input.BusinessTaxPeriodId) + 1;
            _db.PriorYearSoldSuspendedVehicles.Add(new PriorYearSoldSuspendedVehicle
            {
                BusinessTaxPeriodId = input.BusinessTaxPeriodId,
                UnitNumber = nextUnitNumber,
                Vin = input.Vin,
                MileageLimit = input.MileageLimit,
                DateSold = input.DateSold!.Value,
                BuyerName = input.BuyerName
            });
        }

        _db.SaveChanges();

        return RedirectToAction("PriorYearSoldSuspendedVehicle", new { businessTaxPeriodId = input.BusinessTaxPeriodId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePriorYearSoldSuspendedVehicle(int id, int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var owns = _db.BusinessTaxPeriods.Any(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);
        if (!owns)
        {
            return NotFound();
        }

        var vehicle = _db.PriorYearSoldSuspendedVehicles.FirstOrDefault(v => v.Id == id && v.BusinessTaxPeriodId == businessTaxPeriodId);
        if (vehicle != null)
        {
            _db.PriorYearSoldSuspendedVehicles.Remove(vehicle);
            _db.SaveChanges();

            var remaining = _db.PriorYearSoldSuspendedVehicles
                .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
                .OrderBy(v => v.UnitNumber)
                .ToList();

            for (int i = 0; i < remaining.Count; i++)
            {
                remaining[i].UnitNumber = i + 1;
            }
            _db.SaveChanges();
        }

        return RedirectToAction("PriorYearSoldSuspendedVehicle", new { businessTaxPeriodId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NextFromPriorYearSoldSuspendedVehicle(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        var hasVehicles = _db.PriorYearSoldSuspendedVehicles.Any(v => v.BusinessTaxPeriodId == businessTaxPeriodId);
        if (!hasVehicles)
        {
            TempData["PriorYearSoldSuspendedVehicleError"] = "Add at least one vehicle before continuing.";
            return RedirectToAction("PriorYearSoldSuspendedVehicle", new { businessTaxPeriodId });
        }

        return RouteToNextVehicleStep(taxPeriod, "PriorYearSoldSuspended");
    }

    /// <summary>
    /// Finds the Previous Step target for a vehicle-category screen: walks backward through
    /// included categories (Taxable -> Suspended -> Credit -> Prior Year Sold Suspended) and
    /// returns the closest included one before the current screen, or Filing Category if none.
    /// Also reused as-is by IrsPayment and Review to find their "Previous Step" target.
    /// </summary>
    private string GetPreviousVehicleStepUrl(BusinessTaxPeriod taxPeriod, string currentStep)
    {
        var order = new[] { "Taxable", "Suspended", "Credit", "PriorYearSoldSuspended" };
        var currentIndex = Array.IndexOf(order, currentStep);

        for (int i = currentIndex - 1; i >= 0; i--)
        {
            var included = order[i] switch
            {
                "Taxable" => taxPeriod.IncludeTaxable,
                "Suspended" => taxPeriod.IncludeSuspended,
                "Credit" => taxPeriod.IncludeCredit,
                "PriorYearSoldSuspended" => taxPeriod.IncludePriorYearSoldSuspended,
                _ => false
            };

            if (included)
            {
                var actionName = order[i] switch
                {
                    "Taxable" => "TaxableVehicle",
                    "Suspended" => "SuspendedVehicle",
                    "Credit" => "CreditVehicle",
                    "PriorYearSoldSuspended" => "PriorYearSoldSuspendedVehicle",
                    _ => throw new InvalidOperationException()
                };
                return Url.Action(actionName, new { businessTaxPeriodId = taxPeriod.Id })!;
            }
        }

        return Url.Action("FilingCategory", new { businessTaxPeriodId = taxPeriod.Id })!;
    }

    /// <summary>
    /// Routes to the next selected vehicle-category screen in order (Taxable -> Suspended ->
    /// Credit -> Prior Year Sold Suspended), starting after whichever step was just completed.
    /// Falls through to IRS Payment once no more vehicle categories remain.
    /// </summary>
    private IActionResult RouteToNextVehicleStep(BusinessTaxPeriod taxPeriod, string afterStep)
    {
        var order = new[] { "Taxable", "Suspended", "Credit", "PriorYearSoldSuspended" };
        var startIndex = afterStep == "start" ? 0 : Array.IndexOf(order, afterStep) + 1;

        for (int i = startIndex; i < order.Length; i++)
        {
            var included = order[i] switch
            {
                "Taxable" => taxPeriod.IncludeTaxable,
                "Suspended" => taxPeriod.IncludeSuspended,
                "Credit" => taxPeriod.IncludeCredit,
                "PriorYearSoldSuspended" => taxPeriod.IncludePriorYearSoldSuspended,
                _ => false
            };

            if (included)
            {
                var actionName = order[i] switch
                {
                    "Taxable" => "TaxableVehicle",
                    "Suspended" => "SuspendedVehicle",
                    "Credit" => "CreditVehicle",
                    "PriorYearSoldSuspended" => "PriorYearSoldSuspendedVehicle",
                    _ => throw new InvalidOperationException()
                };
                return RedirectToAction(actionName, new { businessTaxPeriodId = taxPeriod.Id });
            }
        }

        return RedirectToAction("IrsPayment", new { businessTaxPeriodId = taxPeriod.Id });
    }

    private PriorYearSoldSuspendedVehicleListViewModel BuildPriorYearSoldSuspendedVehicleListViewModel(int businessTaxPeriodId)
    {
        var vehicles = _db.PriorYearSoldSuspendedVehicles
            .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
            .OrderBy(v => v.UnitNumber)
            .Select(v => new PriorYearSoldSuspendedVehicleRowViewModel
            {
                Id = v.Id,
                UnitNumber = v.UnitNumber,
                Vin = v.Vin,
                MileageLimit = v.MileageLimit,
                DateSold = v.DateSold,
                BuyerName = v.BuyerName
            })
            .ToList();

        return new PriorYearSoldSuspendedVehicleListViewModel
        {
            BusinessTaxPeriodId = businessTaxPeriodId,
            Vehicles = vehicles
        };
    }

    private SuspendedVehicleListViewModel BuildSuspendedVehicleListViewModel(int businessTaxPeriodId)
    {
        var vehicles = _db.SuspendedVehicles
            .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
            .OrderBy(v => v.UnitNumber)
            .Select(v => new SuspendedVehicleRowViewModel
            {
                Id = v.Id,
                UnitNumber = v.UnitNumber,
                Vin = v.Vin,
                MileageLimit = v.MileageLimit
            })
            .ToList();

        return new SuspendedVehicleListViewModel
        {
            BusinessTaxPeriodId = businessTaxPeriodId,
            Vehicles = vehicles
        };
    }

    private CreditVehicleListViewModel BuildCreditVehicleListViewModel(int businessTaxPeriodId)
    {
        var vehicles = _db.CreditVehicles
            .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
            .OrderBy(v => v.UnitNumber)
            .Select(v => new CreditVehicleRowViewModel
            {
                Id = v.Id,
                UnitNumber = v.UnitNumber,
                Vin = v.Vin,
                WeightCategory = v.WeightCategory,
                IsLogging = v.IsLogging,
                Reason = v.Reason,
                EffectiveDate = v.EffectiveDate,
                PreviouslyReportedTax = v.PreviouslyReportedTax,
                TaxAmountUsed = v.TaxAmountUsed,
                CreditAmount = v.CreditAmount
            })
            .ToList();

        return new CreditVehicleListViewModel
        {
            BusinessTaxPeriodId = businessTaxPeriodId,
            Vehicles = vehicles
        };
    }

    private TaxableVehicleListViewModel BuildTaxableVehicleListViewModel(int businessTaxPeriodId)
    {
        var vehicles = _db.TaxableVehicles
            .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
            .OrderBy(v => v.UnitNumber)
            .Select(v => new TaxableVehicleRowViewModel
            {
                Id = v.Id,
                UnitNumber = v.UnitNumber,
                Vin = v.Vin,
                WeightCategory = v.WeightCategory,
                WeightCategoryLabel = v.WeightCategory,
                IsLogging = v.IsLogging,
                TaxAmount = v.TaxAmount
            })
            .ToList();

        return new TaxableVehicleListViewModel
        {
            BusinessTaxPeriodId = businessTaxPeriodId,
            Vehicles = vehicles,
            NewVehicle = new TaxableVehicleInputViewModel { BusinessTaxPeriodId = businessTaxPeriodId }
        };
    }

    private static int GetCurrentIrsTaxYearStart()
    {
        var now = DateTime.UtcNow;
        return now.Month >= 7 ? now.Year : now.Year - 1;
    }

    // ================================================================
    // IRS PAYMENT (EFW / EFTPS / Credit-Debit-to-IRS) — Step 4
    // ================================================================

    [HttpGet]
    public IActionResult IrsPayment(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        var (totalTax, totalCredit, balance) = ComputePaymentTotals(businessTaxPeriodId);

        var existingPayment = _db.PaymentInfos.FirstOrDefault(p => p.BusinessTaxPeriodId == businessTaxPeriodId);

        ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
        ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "PriorYearSoldSuspended");

        var model = new IrsPaymentViewModel
        {
            BusinessTaxPeriodId = businessTaxPeriodId,
            TotalTaxAmount = totalTax,
            TotalCreditAmount = totalCredit,
            BalanceDue = balance,
            Payment = existingPayment == null
                ? new PaymentInputViewModel { BusinessTaxPeriodId = businessTaxPeriodId }
                : new PaymentInputViewModel
                {
                    BusinessTaxPeriodId = businessTaxPeriodId,
                    PaymentMethod = existingPayment.PaymentMethod,
                    AccountType = existingPayment.AccountType,
                    AccountNumber = existingPayment.AccountNumber,
                    PhoneNumber = existingPayment.PhoneNumber,
                    RoutingNumber = existingPayment.RoutingNumber
                }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitPayment(PaymentInputViewModel input)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == input.BusinessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        if (input.PaymentMethod == "EFW")
        {
            if (string.IsNullOrWhiteSpace(input.AccountType))
                ModelState.AddModelError(nameof(input.AccountType), "Please select Checking or Saving.");
            if (string.IsNullOrWhiteSpace(input.AccountNumber))
                ModelState.AddModelError(nameof(input.AccountNumber), "Account number is required.");
            if (string.IsNullOrWhiteSpace(input.PhoneNumber))
                ModelState.AddModelError(nameof(input.PhoneNumber), "Phone number is required.");

            if (string.IsNullOrWhiteSpace(input.RoutingNumber))
            {
                ModelState.AddModelError(nameof(input.RoutingNumber), "Routing number is required.");
            }
            else if (!IsValidAbaRoutingNumber(input.RoutingNumber))
            {
                ModelState.AddModelError(nameof(input.RoutingNumber),
                    "This doesn't look like a valid routing number. Double-check the 9 digits at the bottom-left of your check.");
            }
        }
        else if (input.PaymentMethod != "EFTPS" && input.PaymentMethod != "CreditCard")
        {
            ModelState.AddModelError(nameof(input.PaymentMethod), "Please select a payment method.");
        }

        var (totalTax, totalCredit, balance) = ComputePaymentTotals(input.BusinessTaxPeriodId);

        if (!ModelState.IsValid)
        {
            ViewBag.BusinessName = taxPeriod.Business!.BusinessName;
            ViewBag.PreviousUrl = GetPreviousVehicleStepUrl(taxPeriod, "PriorYearSoldSuspended");

            var errorModel = new IrsPaymentViewModel
            {
                BusinessTaxPeriodId = input.BusinessTaxPeriodId,
                TotalTaxAmount = totalTax,
                TotalCreditAmount = totalCredit,
                BalanceDue = balance,
                Payment = input
            };
            return View("IrsPayment", errorModel);
        }

        var payment = _db.PaymentInfos.FirstOrDefault(p => p.BusinessTaxPeriodId == input.BusinessTaxPeriodId);
        if (payment == null)
        {
            payment = new PaymentInfo { BusinessTaxPeriodId = input.BusinessTaxPeriodId };
            _db.PaymentInfos.Add(payment);
        }

        payment.PaymentMethod = input.PaymentMethod;
        payment.AccountType = input.PaymentMethod == "EFW" ? input.AccountType : null;
        payment.AccountNumber = input.PaymentMethod == "EFW" ? input.AccountNumber : null;
        payment.PhoneNumber = input.PaymentMethod == "EFW" ? input.PhoneNumber : null;
        payment.RoutingNumber = input.PaymentMethod == "EFW" ? input.RoutingNumber : null;
        payment.TotalTaxAmount = totalTax;
        payment.TotalCreditAmount = totalCredit;
        payment.BalanceDue = balance;
        payment.CreatedAt = DateTime.UtcNow;

        taxPeriod.CurrentStep = 5;
        taxPeriod.UpdatedAt = DateTime.UtcNow;

        _db.SaveChanges();

        if (input.PaymentMethod == "CreditCard")
        {
            TempData["PayByCardNotice"] =
                "The IRS handles card payments directly. You'll be redirected to IRS.gov/PayByCard to complete this payment.";
        }

        return RedirectToAction("Review", new { businessTaxPeriodId = taxPeriod.Id });
    }

    /// <summary>
    /// Total tax owed (sum of Taxable Vehicle tax) minus total credit (sum of Credit Vehicle
    /// credit amounts), floored at zero. Suspended and Prior Year Sold Suspended vehicles carry
    /// no tax under Form 2290 and are intentionally excluded from this calculation.
    /// </summary>
    /// <summary>
    /// Validates a 9-digit ABA routing number using the standard bank checksum formula:
    /// 3*(d1+d4+d7) + 7*(d2+d5+d8) + 1*(d3+d6+d9), valid when the result is a multiple of 10.
    /// This catches typos (transposed/mistyped digits) — it does not confirm the routing
    /// number belongs to a real, currently-operating bank.
    /// </summary>
    private static bool IsValidAbaRoutingNumber(string routingNumber)
    {
        if (string.IsNullOrWhiteSpace(routingNumber) || routingNumber.Length != 9 || !routingNumber.All(char.IsDigit))
        {
            return false;
        }

        var d = routingNumber.Select(c => c - '0').ToArray();
        var checksum =
            3 * (d[0] + d[3] + d[6]) +
            7 * (d[1] + d[4] + d[7]) +
            1 * (d[2] + d[5] + d[8]);

        return checksum % 10 == 0;
    }

    private (decimal totalTax, decimal totalCredit, decimal balance) ComputePaymentTotals(int businessTaxPeriodId)
    {
        var totalTax = _db.TaxableVehicles
            .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
            .Sum(v => (decimal?)v.TaxAmount) ?? 0m;

        var totalCredit = _db.CreditVehicles
            .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
            .Sum(v => (decimal?)v.CreditAmount) ?? 0m;

        var balance = totalTax - totalCredit;
        if (balance < 0) balance = 0m;

        return (totalTax, totalCredit, balance);
    }

    // ================================================================
    // REVIEW + SERVICE FEE (Stripe) — Step 5
    // ================================================================

    [HttpGet]
    public IActionResult Review(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        var business = taxPeriod.Business!;
        var (totalTax, totalCredit, balance) = ComputePaymentTotals(businessTaxPeriodId);
        var irsPayment = _db.PaymentInfos.FirstOrDefault(p => p.BusinessTaxPeriodId == businessTaxPeriodId);
        var (feeAmount, _) = GetBase2290FilingFee(businessTaxPeriodId);

        ViewBag.BusinessName = business.BusinessName;
        ViewBag.PreviousUrl = Url.Action("IrsPayment", new { businessTaxPeriodId });

        var model = new ReviewViewModel
        {
            BusinessTaxPeriodId = businessTaxPeriodId,
            BusinessName = business.BusinessName,
            Ein = business.Ein,
            AddressLine1 = business.AddressLine1,
            City = business.City,
            State = business.State,
            ZipCode = business.ZipCode,
            TaxYearStart = taxPeriod.TaxYearStart,
            TaxYearEnd = taxPeriod.TaxYearEnd,
            FirstUsedMonth = taxPeriod.FirstUsedMonth,
            TaxableCount = _db.TaxableVehicles.Count(v => v.BusinessTaxPeriodId == businessTaxPeriodId),
            SuspendedCount = _db.SuspendedVehicles.Count(v => v.BusinessTaxPeriodId == businessTaxPeriodId),
            CreditCount = _db.CreditVehicles.Count(v => v.BusinessTaxPeriodId == businessTaxPeriodId),
            PriorYearSoldSuspendedCount = _db.PriorYearSoldSuspendedVehicles.Count(v => v.BusinessTaxPeriodId == businessTaxPeriodId),
            IrsPaymentMethod = irsPayment?.PaymentMethod ?? "Not selected",
            TotalTaxAmount = totalTax,
            TotalCreditAmount = totalCredit,
            BalanceDue = balance,
            ServiceFeeAmount = feeAmount,
            OtherCharges = 0m,
            StripePublishableKey = _config["Stripe:PublishableKey"] ?? string.Empty,
            DefaultAddressLine1 = business.AddressLine1,
            DefaultCity = business.City,
            DefaultState = business.State,
            DefaultZip = business.ZipCode,
            DefaultPhone = business.PhoneNumber
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApplyDiscountCode([FromBody] ApplyDiscountCodeRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var owns = _db.BusinessTaxPeriods.Any(t => t.Id == request.BusinessTaxPeriodId && t.Business!.UserId == userId);
        if (!owns)
        {
            return NotFound();
        }

        var (feeAmount, _) = GetBase2290FilingFee(request.BusinessTaxPeriodId);
        var code = (request.Code ?? string.Empty).Trim().ToUpperInvariant();

        var discount = _db.DiscountCodes.FirstOrDefault(d =>
            d.Code.ToUpper() == code && d.IsActive &&
            (d.ExpiresAt == null || d.ExpiresAt >= DateTime.UtcNow));

        if (discount == null)
        {
            return Json(new { success = false, message = "That discount code is invalid or expired." });
        }

        var discountAmount = discount.FlatAmountOff ?? Math.Round(feeAmount * (discount.PercentOff ?? 0) / 100m, 2);
        if (discountAmount > feeAmount) discountAmount = feeAmount;

        return Json(new
        {
            success = true,
            discountAmount,
            total = feeAmount - discountAmount
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateServiceFeeIntent([FromBody] CreateServiceFeeIntentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var owns = _db.BusinessTaxPeriods.Any(t => t.Id == request.BusinessTaxPeriodId && t.Business!.UserId == userId);
        if (!owns)
        {
            return NotFound();
        }

        var total = ComputeServiceFeeTotal(request.BusinessTaxPeriodId, request.DiscountCode);
        var (_, stripePriceId) = GetBase2290FilingFee(request.BusinessTaxPeriodId);

        var service = new PaymentIntentService();
        var intent = service.Create(new PaymentIntentCreateOptions
        {
            // Stripe expects the smallest currency unit (cents).
            Amount = (long)Math.Round(total * 100m, MidpointRounding.AwayFromZero),
            Currency = "usd",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            Metadata = new Dictionary<string, string>
            {
                { "businessTaxPeriodId", request.BusinessTaxPeriodId.ToString() },
                { "stripePriceId", stripePriceId }
            }
        });

        return Json(new { clientSecret = intent.ClientSecret, total });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmServiceFee([FromBody] ConfirmServiceFeeRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == request.BusinessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        // Verify server-side against Stripe — never trust the client's claim that payment succeeded.
        var intentService = new PaymentIntentService();
        var intent = intentService.Get(request.PaymentIntentId, new PaymentIntentGetOptions
        {
            Expand = new List<string> { "payment_method" }
        });

        if (intent.Status != "succeeded")
        {
            return Json(new { success = false, message = "Payment was not completed. Please try again." });
        }

        var card = intent.PaymentMethod?.Card;
        var (feeAmount, _) = GetBase2290FilingFee(request.BusinessTaxPeriodId);
        var total = ComputeServiceFeeTotal(request.BusinessTaxPeriodId, request.DiscountCode);
        var discountAmount = feeAmount - total;

        var payment = _db.ServiceFeePayments.FirstOrDefault(p => p.BusinessTaxPeriodId == request.BusinessTaxPeriodId)
                      ?? new ServiceFeePayment { BusinessTaxPeriodId = request.BusinessTaxPeriodId };

        payment.StripeCustomerId = intent.CustomerId ?? string.Empty;
        payment.StripePaymentIntentId = intent.Id;
        payment.StripePaymentMethodId = intent.PaymentMethodId ?? string.Empty;
        payment.CardBrand = card?.Brand ?? string.Empty;
        payment.Last4 = card?.Last4 ?? string.Empty;
        payment.ExpMonth = (int)(card?.ExpMonth ?? 0);
        payment.ExpYear = (int)(card?.ExpYear ?? 0);
        payment.ServiceFeeAmount = feeAmount;
        payment.OtherCharges = 0m;
        payment.DiscountAmount = discountAmount;
        payment.TotalCharged = total;
        payment.DiscountCode = string.IsNullOrWhiteSpace(request.DiscountCode) ? null : request.DiscountCode.Trim().ToUpperInvariant();
        payment.BillingName = $"{request.FirstName} {request.LastName}".Trim();
        payment.BillingAddressLine1 = request.AddressLine1;
        payment.BillingAddressLine2 = request.AddressLine2;
        payment.BillingCity = request.City;
        payment.BillingState = request.State;
        payment.BillingZip = request.Zip;
        payment.ContactPhone = request.Phone;
        payment.ContactEmail = request.Email;
        payment.Status = "Succeeded";
        payment.CreatedAt = DateTime.UtcNow;

        if (payment.Id == 0)
        {
            _db.ServiceFeePayments.Add(payment);
        }

        // Paying the service fee is the final act of submission — mark the filing done
        // and kick off IRS transmission (handed off to IMefTransmissionService; the actual
        // IRS MeF ATS/production transmission is built separately).
        taxPeriod.Status = "Submitted";
        taxPeriod.CurrentStep = 6;
        taxPeriod.IrsSubmissionStatus = "Processing";
        taxPeriod.IrsConfirmationNumber = $"TTP-{DateTime.UtcNow:yyyyMMdd}-{taxPeriod.Id:D6}";
        taxPeriod.UpdatedAt = DateTime.UtcNow;

        _db.SaveChanges();

        TransmitToMef(taxPeriod.Id);

        return Json(new { success = true, redirectUrl = Url.Action("Finish", new { businessTaxPeriodId = taxPeriod.Id }) });
    }

    private decimal ComputeServiceFeeTotal(int businessTaxPeriodId, string? discountCode)
    {
        var (feeAmount, _) = GetBase2290FilingFee(businessTaxPeriodId);

        if (string.IsNullOrWhiteSpace(discountCode))
        {
            return feeAmount;
        }

        var code = discountCode.Trim().ToUpperInvariant();
        var discount = _db.DiscountCodes.FirstOrDefault(d =>
            d.Code.ToUpper() == code && d.IsActive &&
            (d.ExpiresAt == null || d.ExpiresAt >= DateTime.UtcNow));

        if (discount == null)
        {
            return feeAmount;
        }

        var discountAmount = discount.FlatAmountOff ?? Math.Round(feeAmount * (discount.PercentOff ?? 0) / 100m, 2);
        if (discountAmount > feeAmount) discountAmount = feeAmount;

        return feeAmount - discountAmount;
    }

    /// <summary>
    /// The base 2290 e-file fee, tiered by total vehicle count across all four categories
    /// (Taxable + Suspended + Credit + Prior Year Sold Suspended) for this filing.
    /// Fleets larger than the top configured bracket fall back to the highest tier.
    /// </summary>
    private (decimal amount, string stripePriceId) GetBase2290FilingFee(int businessTaxPeriodId)
    {
        var vehicleCount =
            _db.TaxableVehicles.Count(v => v.BusinessTaxPeriodId == businessTaxPeriodId) +
            _db.SuspendedVehicles.Count(v => v.BusinessTaxPeriodId == businessTaxPeriodId) +
            _db.CreditVehicles.Count(v => v.BusinessTaxPeriodId == businessTaxPeriodId) +
            _db.PriorYearSoldSuspendedVehicles.Count(v => v.BusinessTaxPeriodId == businessTaxPeriodId);

        if (vehicleCount < 1) vehicleCount = 1;

        var tier = _pricing.Filing2290Tiers
            .OrderBy(t => t.MaxVehicles)
            .FirstOrDefault(t => vehicleCount <= t.MaxVehicles);

        if (tier == null && _pricing.Filing2290Tiers.Count == 0)
        {
            throw new InvalidOperationException(
                "Pricing:Filing2290Tiers is empty. Check that Program.cs calls " +
                "builder.Services.Configure<PricingOptions>(builder.Configuration.GetSection(\"Pricing\")) " +
                "and that appsettings.json has a populated Pricing:Filing2290Tiers array.");
        }

        tier ??= _pricing.Filing2290Tiers.OrderByDescending(t => t.MaxVehicles).First();

        return (tier.Amount, tier.StripePriceId);
    }

    // ================================================================
    // FINISH + IRS MeF HANDOFF — Step 6
    // ================================================================

    [HttpGet]
    public IActionResult Finish(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        ViewBag.BusinessName = taxPeriod.Business?.BusinessName;

        return View(new FinishViewModel
        {
            BusinessTaxPeriodId = businessTaxPeriodId,
            ConfirmationNumber = taxPeriod.IrsConfirmationNumber ?? string.Empty,
            Status = taxPeriod.IrsSubmissionStatus,
            Schedule1Url = taxPeriod.IrsSubmissionStatus == "Accepted"
                ? Url.Action("DownloadScheduleOnePlaceholder", new { businessTaxPeriodId })
                : null
        });
    }

    [HttpGet]
    public IActionResult GetFilingStatus(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null)
        {
            return NotFound();
        }

        return Json(new
        {
            status = taxPeriod.IrsSubmissionStatus,
            confirmationNumber = taxPeriod.IrsConfirmationNumber,
            schedule1Url = taxPeriod.IrsSubmissionStatus == "Accepted"
                ? Url.Action("DownloadScheduleOnePlaceholder", new { businessTaxPeriodId })
                : null
        });
    }

    /// <summary>
    /// PLACEHOLDER — serves a clearly-marked stub file so the download button is functional
    /// end-to-end during development. Replace with the actual IRS-stamped Schedule 1 (returned
    /// by your MeF transmitter) once real e-file transmission is wired in.
    /// </summary>
    [HttpGet]
    public IActionResult DownloadScheduleOnePlaceholder(int businessTaxPeriodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var taxPeriod = _db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId && t.Business!.UserId == userId);

        if (taxPeriod == null || taxPeriod.IrsSubmissionStatus != "Accepted")
        {
            return NotFound();
        }

        var placeholderText = $"PLACEHOLDER — Schedule 1 for {taxPeriod.Business?.BusinessName}\n" +
                               $"Confirmation: {taxPeriod.IrsConfirmationNumber}\n" +
                               "This is a stub file. Wire up real IRS MeF transmission to replace it.";
        var bytes = System.Text.Encoding.UTF8.GetBytes(placeholderText);

        return File(bytes, "text/plain", $"Schedule1-Placeholder-{taxPeriod.Id}.txt");
    }

    /// <summary>
    /// Builds the JSON handoff package and calls IMefTransmissionService — this is the
    /// integration seam for your separate MeF developer. Swap PlaceholderMefTransmissionService
    /// for the real implementation (registered once in Program.cs) and this method needs no changes.
    /// </summary>
    private void TransmitToMef(int businessTaxPeriodId)
    {
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var scopedDb = scope.ServiceProvider.GetRequiredService<TruckTaxProDbContext>();
            var mefService = scope.ServiceProvider.GetRequiredService<IMefTransmissionService>();

            var package = BuildFilingPackage(scopedDb, businessTaxPeriodId);
            if (package == null) return;

            var result = await mefService.SubmitAsync(package);

            var taxPeriod = scopedDb.BusinessTaxPeriods.FirstOrDefault(t => t.Id == businessTaxPeriodId);
            if (taxPeriod == null) return;

            taxPeriod.IrsSubmissionStatus = result.Status;
            if (!string.IsNullOrEmpty(result.IrsSubmissionId))
            {
                taxPeriod.IrsConfirmationNumber = result.IrsSubmissionId;
            }
            taxPeriod.UpdatedAt = DateTime.UtcNow;

            scopedDb.SaveChanges();
        });
    }

    private static FilingPackageDto? BuildFilingPackage(TruckTaxProDbContext db, int businessTaxPeriodId)
    {
        var taxPeriod = db.BusinessTaxPeriods
            .Include(t => t.Business)
            .FirstOrDefault(t => t.Id == businessTaxPeriodId);

        if (taxPeriod == null || taxPeriod.Business == null) return null;

        var business = taxPeriod.Business;
        var irsPayment = db.PaymentInfos.FirstOrDefault(p => p.BusinessTaxPeriodId == businessTaxPeriodId);

        var totalTax = db.TaxableVehicles
            .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
            .Sum(v => (decimal?)v.TaxAmount) ?? 0m;

        var totalCredit = db.CreditVehicles
            .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
            .Sum(v => (decimal?)v.CreditAmount) ?? 0m;

        var balance = totalTax - totalCredit;
        if (balance < 0) balance = 0m;

        return new FilingPackageDto
        {
            BusinessTaxPeriodId = businessTaxPeriodId,
            ConfirmationNumber = taxPeriod.IrsConfirmationNumber ?? string.Empty,

            Business = new FilingBusinessDto
            {
                BusinessName = business.BusinessName,
                Ein = business.Ein,
                BusinessType = business.BusinessType,
                AddressLine1 = business.AddressLine1,
                City = business.City,
                State = business.State,
                ZipCode = business.ZipCode,
                PhoneNumber = business.PhoneNumber
            },

            TaxPeriod = new FilingTaxPeriodDto
            {
                TaxYearStart = taxPeriod.TaxYearStart,
                TaxYearEnd = taxPeriod.TaxYearEnd,
                FirstUsedMonth = taxPeriod.FirstUsedMonth,
                IsFinalReturn = taxPeriod.IsFinalReturn,
                ConsentToDisclosure = taxPeriod.ConsentToDisclosure
            },

            TaxableVehicles = db.TaxableVehicles
                .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
                .OrderBy(v => v.UnitNumber)
                .Select(v => new FilingTaxableVehicleDto
                {
                    UnitNumber = v.UnitNumber,
                    Vin = v.Vin,
                    WeightCategory = v.WeightCategory,
                    IsLogging = v.IsLogging,
                    TaxAmount = v.TaxAmount
                })
                .ToList(),

            SuspendedVehicles = db.SuspendedVehicles
                .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
                .OrderBy(v => v.UnitNumber)
                .Select(v => new FilingSuspendedVehicleDto
                {
                    UnitNumber = v.UnitNumber,
                    Vin = v.Vin,
                    MileageLimit = v.MileageLimit
                })
                .ToList(),

            CreditVehicles = db.CreditVehicles
                .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
                .OrderBy(v => v.UnitNumber)
                .Select(v => new FilingCreditVehicleDto
                {
                    UnitNumber = v.UnitNumber,
                    Vin = v.Vin,
                    WeightCategory = v.WeightCategory,
                    IsLogging = v.IsLogging,
                    Reason = v.Reason,
                    EffectiveDate = v.EffectiveDate,
                    BuyerName = v.BuyerName,
                    FirstUsedMonthPriorYear = v.FirstUsedMonthPriorYear,
                    PreviouslyReportedTax = v.PreviouslyReportedTax,
                    TaxAmountUsed = v.TaxAmountUsed,
                    CreditAmount = v.CreditAmount
                })
                .ToList(),

            PriorYearSoldSuspendedVehicles = db.PriorYearSoldSuspendedVehicles
                .Where(v => v.BusinessTaxPeriodId == businessTaxPeriodId)
                .OrderBy(v => v.UnitNumber)
                .Select(v => new FilingPriorYearSoldSuspendedVehicleDto
                {
                    UnitNumber = v.UnitNumber,
                    Vin = v.Vin,
                    MileageLimit = v.MileageLimit,
                    DateSold = v.DateSold,
                    BuyerName = v.BuyerName
                })
                .ToList(),

            IrsPayment = new FilingIrsPaymentDto
            {
                PaymentMethod = irsPayment?.PaymentMethod ?? string.Empty,
                AccountType = irsPayment?.AccountType,
                AccountNumber = irsPayment?.AccountNumber,
                RoutingNumber = irsPayment?.RoutingNumber,
                PhoneNumber = irsPayment?.PhoneNumber
            },

            TotalTaxAmount = totalTax,
            TotalCreditAmount = totalCredit,
            BalanceDue = balance
        };
    }

    // ================================================================
    // PRIOR-YEAR VEHICLE UPLOAD (existing, unchanged)
    // ================================================================

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
