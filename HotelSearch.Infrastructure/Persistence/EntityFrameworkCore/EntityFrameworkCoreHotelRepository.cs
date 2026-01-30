using HotelSearch.Application.Interfaces;
using HotelSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelSearch.Infrastructure.Persistence.EntityFrameworkCore;

public class EntityFrameworkCoreHotelRepository : IHotelRepository
{
    private readonly HotelSearchDbContext _dbContext;
    private readonly ILogger<EntityFrameworkCoreHotelRepository> _logger;

    public EntityFrameworkCoreHotelRepository(HotelSearchDbContext dbContext, ILogger<EntityFrameworkCoreHotelRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hotels
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hotels
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Hotel> AddAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        if (hotel is null)
            throw new ArgumentNullException(nameof(hotel));

        _dbContext.Hotels.Add(hotel);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogDebug("Hotel with ID: {HotelId} added to database", hotel.Id);
        
        return hotel;
    }

    public async Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        if (hotel is null)
            throw new ArgumentNullException(nameof(hotel));

        _dbContext.Hotels.Update(hotel);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogDebug("Hotel with ID: {HotelId} updated in database", hotel.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _dbContext.Hotels
            .Where(h => h.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        
        _logger.LogDebug("Hotel with ID: {HotelId} deleted from database", id);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hotels
            .AnyAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAndLocationAsync(string name, double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hotels
            .AnyAsync(h => 
                    EF.Functions.ILike(h.Name, name) && 
                    Math.Abs(h.Location.Latitude - latitude) < 0.000001 && 
                    Math.Abs(h.Location.Longitude - longitude) < 0.000001, 
                cancellationToken);
    }
}