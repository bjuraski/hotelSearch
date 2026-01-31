using HotelSearch.Domain.Entities;

namespace HotelSearch.Application.Interfaces;

/// <summary>
/// Repository interface for hotel persistence operations
/// </summary>
public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Hotel> AddAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndLocationAsync(string name, double latitude, double longitude, Guid? excludeId = null, CancellationToken cancellationToken = default);
}