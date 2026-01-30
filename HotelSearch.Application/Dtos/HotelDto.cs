namespace HotelSearch.Application.Dtos;

public record HotelDto(
    Guid Id,
    string Name,
    decimal Price,
    double Latitude,
    double Longitude,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateHotelDto(
    string Name,
    decimal Price,
    double Latitude,
    double Longitude
);

public record UpdateHotelDto(
    string Name,
    decimal Price,
    double Latitude,
    double Longitude
);