using CurrencyConverterAPI.Services.Interfaces;
using CurrencyConverterAPI.Services;
using CurrencyConverterAPI.Repositories;
using CurrencyConverterAPI.Providers;
using CurrencyConverterAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace CurrencyConverterAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyConverterServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<FrankfurterCurrencyProvider>();
        services.AddScoped<ICurrencyProviderFactory, CurrencyProviderFactory>();

        return services;
    }

    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        
        return services;
    }
}