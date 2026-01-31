using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Hotel name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Hotel name must be between 1 and 200 characters")]
    string Name,

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative value")]
    decimal Price,

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    double Latitude,

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    double Longitude
);

public record UpdateHotelDto(
    [Required(ErrorMessage = "Hotel name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Hotel name must be between 1 and 200 characters")]
    string Name,

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative value")]
    decimal Price,

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    double Latitude,

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    double Longitude
);