using HotelSearch.Application.Dtos;
using HotelSearch.Domain.Entities;
using HotelSearch.Domain.ValueObjects;

namespace HotelSearch.Application.Mappings;

/// <summary>
/// Custom mapping extensions for Hotel domain entity and DTOs
/// </summary>
public static class HotelMappings
{
    public static HotelDto ToDto(this Hotel hotel)
    {
        return new HotelDto(
            hotel.Id,
            hotel.Name,
            hotel.Price,
            hotel.Location.Latitude,
            hotel.Location.Longitude,
            hotel.CreatedAt,
            hotel.UpdatedAt
        );
    }

    public static Hotel ToDomain(this CreateHotelDto createHotelDto)
    {
        var location = new GeoLocation(createHotelDto.Latitude, createHotelDto.Longitude);
        return new Hotel(createHotelDto.Name, createHotelDto.Price, location);
    }
    
    public static Hotel ToDomainUpdate(this Hotel hotel, UpdateHotelDto updateHotelDto)
    {
        var location = new GeoLocation(updateHotelDto.Latitude, updateHotelDto.Longitude);
        hotel.Update(updateHotelDto.Name, updateHotelDto.Price, location);
        
        return hotel;
    }
}