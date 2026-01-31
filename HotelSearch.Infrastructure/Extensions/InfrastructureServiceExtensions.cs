using HotelSearch.Application.Interfaces;
using HotelSearch.Infrastructure.Persistence;
using HotelSearch.Infrastructure.Persistence.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSearch.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHotelSearchDbContextFactory, HotelSearchDbContextFactory>();
        services.AddDbContext<HotelSearchDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("HotelSearchDbConnection"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null)));
        
        services.AddScoped<IHotelRepository, EntityFrameworkCoreHotelRepository>();
        
        return services;
    }

    public static IServiceCollection AddInfrastructureInMemory(this IServiceCollection services)
    {
        services.AddSingleton<IHotelRepository, InMemoryHotelRepository>();
        return services;
    }
}