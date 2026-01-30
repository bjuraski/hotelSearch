using HotelSearch.Application.Dtos;

namespace HotelSearch.Application.Services.Interfaces;

/// <summary>
/// Service interface for hotel business operations
/// </summary>
public interface IHotelService
{
    Task<HotelDto> CreateAsync(CreateHotelDto createHotelDto, CancellationToken cancellationToken = default);
    Task<HotelDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<HotelDto> UpdateAsync(Guid id, UpdateHotelDto updateHotelDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<HotelSearchResultDto>> SearchAsync(HotelSearchQuery hotelSearchQuery, CancellationToken cancellationToken = default);
}