namespace HotelSearch.Infrastructure.Persistence.EntityFrameworkCore;

public interface IHotelSearchDbContextFactory
{
    HotelSearchDbContext Create();
    HotelSearchDbContext CreateReadOnly();
}