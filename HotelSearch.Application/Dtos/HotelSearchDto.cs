using System.ComponentModel.DataAnnotations;

namespace HotelSearch.Application.Dtos;

/// <summary>
/// Query parameters for hotel search
/// </summary>
public record HotelSearchQuery(
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    double Latitude,

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    double Longitude,

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
    int PageNumber = 1,

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    int PageSize = 10
);

/// <summary>
/// Search result containing hotel information and distance from search location
/// </summary>
public record HotelSearchResultDto(
    Guid Id,
    string Name,
    decimal Price,
    double Latitude,
    double Longitude,
    double DistanceKm
);

/// <summary>
/// Generic paged result wrapper
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);