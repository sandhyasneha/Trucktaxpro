using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TruckTaxPro.Domain;

namespace TruckTaxPro.Data;


public class TruckTaxProDbContext : IdentityDbContext<ApplicationUser>
{
    public TruckTaxProDbContext(DbContextOptions<TruckTaxProDbContext> options)
        : base(options) { }
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<TaxPeriod> TaxPeriods => Set<TaxPeriod>();
    public DbSet<BusinessTaxPeriod> BusinessTaxPeriods => Set<BusinessTaxPeriod>();
    public DbSet<TaxableVehicle> TaxableVehicles => Set<TaxableVehicle>();
    public DbSet<SuspendedVehicle> SuspendedVehicles => Set<SuspendedVehicle>();
    public DbSet<CreditVehicle> CreditVehicles => Set<CreditVehicle>();
    public DbSet<PriorYearSoldSuspendedVehicle> PriorYearSoldSuspendedVehicles => Set<PriorYearSoldSuspendedVehicle>();
    public DbSet<PaymentInfo> PaymentInfos => Set<PaymentInfo>();
    public DbSet<ServiceFeePayment> ServiceFeePayments => Set<ServiceFeePayment>();
    public DbSet<DiscountCode> DiscountCodes => Set<DiscountCode>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
}