using HotelSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelSearch.Infrastructure.Persistence.EntityFrameworkCore;

public sealed class HotelSearchDbContext : DbContext
{
    private readonly bool _isReadOnly;
    
    public HotelSearchDbContext(DbContextOptions<HotelSearchDbContext> options, bool isReadOnly = false) : base(options)
    {
        _isReadOnly = isReadOnly;
        if (_isReadOnly)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.LazyLoadingEnabled = false;
        }
    }
    
    public DbSet<Hotel> Hotels => Set<Hotel>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelSearchDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _isReadOnly 
            ? throw new InvalidOperationException("Cannot save changes on a read-only context.") 
            : base.SaveChangesAsync(cancellationToken);
    }
}