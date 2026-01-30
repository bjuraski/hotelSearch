namespace HotelSearch.Application.Dtos;

/// <summary>
/// Query parameters for hotel search
/// </summary>
public record HotelSearchQuery(
    double Latitude,
    double Longitude,
    int PageNumber = 1,
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