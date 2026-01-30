using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HotelSearch.Infrastructure.Persistence.EntityFrameworkCore;

public class HotelSearchDbContextFactory : IHotelSearchDbContextFactory
{
    private readonly IConfiguration _configuration;

    public HotelSearchDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public HotelSearchDbContext Create()
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelSearchDbContext>();
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("HotelSearchDbConnection"),
            b => b.MigrationsAssembly("HotelSearch.Infrastructure"));

        return new HotelSearchDbContext(optionsBuilder.Options, isReadOnly: false);
    }

    public HotelSearchDbContext CreateReadOnly()
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelSearchDbContext>();
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("HotelSearchDbConnection"),
            b => b.MigrationsAssembly("HotelSearch.Infrastructure"));

        return new HotelSearchDbContext(optionsBuilder.Options, isReadOnly: true);
    }
}