using HotelSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelSearch.Infrastructure.Persistence.EntityFrameworkCore;

public class HotelSearchDbContext : DbContext
{
    public HotelSearchDbContext(DbContextOptions<HotelSearchDbContext> options) : base(options)
    {
    }
    
    public DbSet<Hotel> Hotels => Set<Hotel>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelSearchDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}