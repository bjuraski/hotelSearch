using System.Collections.Concurrent;
using HotelSearch.Application.Interfaces;
using HotelSearch.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HotelSearch.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of hotel repository using ConcurrentDictionary for thread safety.
/// Suitable for development, testing, and demos. Data is lost when application restarts.
/// </summary>
public class InMemoryHotelRepository : IHotelRepository
{
    private readonly ConcurrentDictionary<Guid, Hotel> _hotels;
    private readonly ILogger<InMemoryHotelRepository> _logger;

    public InMemoryHotelRepository(ILogger<InMemoryHotelRepository> logger)
    {
        _hotels = new ConcurrentDictionary<Guid, Hotel>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _hotels.TryGetValue(id, out var hotel);
        return Task.FromResult(hotel);
    }

    public Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Hotel> hotels = _hotels.Values.ToList();
        return Task.FromResult(hotels);
    }

    public Task<Hotel> AddAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        if (hotel is null)
            throw new ArgumentNullException(nameof(hotel));

        if (!_hotels.TryAdd(hotel.Id, hotel))
        {
            _logger.LogError("Failed to add hotel with ID: {HotelId}, Hotel already exists", hotel.Id);
            throw new InvalidOperationException($"Hotel with ID: {hotel.Id} already exists");
        }

        _logger.LogDebug("Hotel with ID: {HotelId} added to in-memory store", hotel.Id);
        return Task.FromResult(hotel);
    }

    public Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        if (hotel is null)
            throw new ArgumentNullException(nameof(hotel));

        _hotels[hotel.Id] = hotel;
        _logger.LogDebug("Hotel with ID: {HotelId} updated in in-memory store", hotel.Id);
        
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _hotels.TryRemove(id, out _);
        _logger.LogDebug("Hotel with ID: {HotelId} removed from in-memory store", id);
        
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_hotels.ContainsKey(id));
    }
}