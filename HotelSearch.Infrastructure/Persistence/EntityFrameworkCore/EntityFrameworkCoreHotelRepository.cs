using HotelSearch.Application.Interfaces;
using HotelSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelSearch.Infrastructure.Persistence.EntityFrameworkCore;

public class EntityFrameworkCoreHotelRepository : IHotelRepository
{
    private readonly IHotelSearchDbContextFactory _dbContextFactory;
    private readonly ILogger<EntityFrameworkCoreHotelRepository> _logger;

    public EntityFrameworkCoreHotelRepository(IHotelSearchDbContextFactory dbContextFactory, ILogger<EntityFrameworkCoreHotelRepository> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateReadOnly();
        return await dbContext.Hotels
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateReadOnly();
        return await dbContext.Hotels
            .ToListAsync(cancellationToken);
    }

    public async Task<Hotel> AddAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        if (hotel is null)
            throw new ArgumentNullException(nameof(hotel));
        
        await using var dbContext = _dbContextFactory.Create();
        dbContext.Hotels.Add(hotel);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogDebug("Hotel with ID: {HotelId} added to database", hotel.Id);
        
        return hotel;
    }

    public async Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        if (hotel is null)
            throw new ArgumentNullException(nameof(hotel));

        await using var dbContext = _dbContextFactory.Create();
        dbContext.Hotels.Update(hotel);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogDebug("Hotel with ID: {HotelId} updated in database", hotel.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.Create();
        await dbContext.Hotels
            .Where(h => h.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        
        _logger.LogDebug("Hotel with ID: {HotelId} deleted from database", id);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateReadOnly();
        return await dbContext.Hotels
            .AnyAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAndLocationAsync(string name, double latitude, double longitude, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateReadOnly();
        return await dbContext.Hotels
            .AnyAsync(h =>
                    (excludeId == null || h.Id != excludeId) &&
                    EF.Functions.ILike(h.Name, name) &&
                    Math.Abs(h.Location.Latitude - latitude) < 0.000001 &&
                    Math.Abs(h.Location.Longitude - longitude) < 0.000001,
                cancellationToken);
    }
}