using HotelSearch.Application.Dtos;
using HotelSearch.Application.Exceptions;
using HotelSearch.Application.Interfaces;
using HotelSearch.Application.Mappings;
using HotelSearch.Application.Services.Interfaces;
using HotelSearch.Domain.Entities;
using HotelSearch.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HotelSearch.Application.Services.Implementations;

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<HotelService> _logger;

    public HotelService(IHotelRepository hotelRepository, ILogger<HotelService> logger)
    {
        _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HotelDto> CreateAsync(CreateHotelDto createHotelDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating hotel: {HotelName}", createHotelDto.Name);

        var hotel = createHotelDto.ToDomain();
        var created = await _hotelRepository.AddAsync(hotel, cancellationToken);

        _logger.LogInformation("Hotel created successfully with ID: {HotelId}", created.Id);

        return created.ToDto();
    }
    
    public async Task<HotelDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id, cancellationToken);
        return hotel?.ToDto();
    }

    public async Task<HotelDto> UpdateAsync(Guid id, UpdateHotelDto updateHotelDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating hotel with ID: {HotelId}", id);

        var hotel = await _hotelRepository.GetByIdAsync(id, cancellationToken);
        if (hotel is null)
            throw new NotFoundException($"Hotel with ID {id} not found");

        hotel = hotel.ToDomainUpdate(updateHotelDto);
        await _hotelRepository.UpdateAsync(hotel, cancellationToken);

        _logger.LogInformation("Hotel with ID: {HotelId} updated successfully", id);

        return hotel.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting hotel with ID: {HotelId}", id);

        var exists = await _hotelRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            _logger.LogWarning("Hotel with ID: {HotelId} not found for deletion", id);
            throw new NotFoundException($"Hotel with ID: {id} not found");
        }

        await _hotelRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Hotel with ID: {HotelId} deleted successfully", id);
    }

    public async Task<PagedResult<HotelSearchResultDto>> SearchAsync(HotelSearchQuery hotelSearchQuery, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching hotels near ({Latitude}, {Longitude}), Page: {PageNumber}, Size: {PageSize}",
            hotelSearchQuery.Latitude, hotelSearchQuery.Longitude, hotelSearchQuery.PageNumber, hotelSearchQuery.PageSize);
        
        var userLocation = new GeoLocation(hotelSearchQuery.Latitude, hotelSearchQuery.Longitude);
        var allHotels = await _hotelRepository.GetAllAsync(cancellationToken);
        
        // Calculate distances and create search results
        var hotelsWithDistance = allHotels
            .Select(h => new HotelWithDistance(h, h.Location.DistanceTo(userLocation)))
            .ToList();
        
        if (hotelsWithDistance.Count == 0)
        {
            _logger.LogInformation("No hotels found");
            return new PagedResult<HotelSearchResultDto>(
                Array.Empty<HotelSearchResultDto>(),
                hotelSearchQuery.PageNumber,
                hotelSearchQuery.PageSize,
                0,
                0
            );
        }
        
        var sorted = hotelsWithDistance
            .OrderBy(h => CalculateScore(h, hotelsWithDistance))
            .ToList();

        var totalCount = sorted.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)hotelSearchQuery.PageSize);
        var paged = sorted
            .Skip((hotelSearchQuery.PageNumber - 1) * hotelSearchQuery.PageSize)
            .Take(hotelSearchQuery.PageSize)
            .Select(result => new HotelSearchResultDto(
                result.Hotel.Id,
                result.Hotel.Name,
                result.Hotel.Price,
                result.Hotel.Location.Latitude,
                result.Hotel.Location.Longitude,
                Math.Round(result.Distance, 2)
            ))
            .ToList();
        
        _logger.LogInformation("Found {TotalCount} hotels, returning page {PageNumber}", totalCount, hotelSearchQuery.PageNumber);

        return new PagedResult<HotelSearchResultDto>(
            paged,
            hotelSearchQuery.PageNumber,
            hotelSearchQuery.PageSize,
            totalCount,
            totalPages
        );
    }
    
    /// <summary>
    /// Calculates normalized score for hotel ranking.
    /// Lower score = better match (closer and cheaper)
    /// </summary>
    private static double CalculateScore(HotelWithDistance hotelWithDistance, List<HotelWithDistance> allHotels)
    {
        // Normalize distance (0-1 scale)
        var maxDistance = allHotels.Max(x => x.Distance);
        var normalizedDistance = maxDistance > 0 ? hotelWithDistance.Distance / maxDistance : 0;

        // Normalize price (0-1 scale)
        var maxPrice = allHotels.Max(x => x.Hotel.Price);
        var normalizedPrice = maxPrice > 0 ? (double)(hotelWithDistance.Hotel.Price / maxPrice) : 0;

        // Combined score (equal weight to distance and price)
        return normalizedDistance + normalizedPrice;
    }
    
    /// <summary>
    /// Helper record for hotel search with calculated distance
    /// </summary>
    private record HotelWithDistance(Hotel Hotel, double Distance);
}