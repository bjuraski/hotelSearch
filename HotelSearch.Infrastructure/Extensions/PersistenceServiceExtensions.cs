using HotelSearch.Application.Interfaces;
using HotelSearch.Infrastructure.Persistence;
using HotelSearch.Infrastructure.Persistence.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSearch.Infrastructure.Extensions;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HotelSearchDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("HotelSearchDbConnection")));

        services.AddSingleton<IHotelRepository, InMemoryHotelRepository>();
        services.AddScoped<IHotelRepository, EntityFrameworkCoreHotelRepository>();
        
        return services;
    }
}