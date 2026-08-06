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
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
}