using HostelListing.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Data;

public class HotelListingDbContext(DbContextOptions<HotelListingDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Country> Countries { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<ApiKey> ApiKey { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApiKey>(b =>
        {
            b.HasIndex(k => k.Key).IsUnique();
        });
    }
}
