using HotelSearch.Application.Services.Implementations;
using HotelSearch.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSearch.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHotelService, HotelService>();
        return services;
    }
}