using CurrencyConverterAPI.Models;
using CurrencyConverterAPI.Services;
using CurrencyConverterAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace CurrencyConverterAPI.Extensions;

public static class UserSeeder
{
    public static async Task SeedUsers(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        
        var adminUser = await userRepository.GetByUsernameAsync("admin");
        if (adminUser == null)
        {
            adminUser = new User
            {
                Username = "admin",
                Role = Roles.Admin
            };
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");
            await userRepository.CreateAsync(adminUser);
        }

        var regularUser = await userRepository.GetByUsernameAsync("user");
        if (regularUser == null)
        {
            regularUser = new User
            {
                Username = "user",
                Role = Roles.User
            };
            regularUser.PasswordHash = passwordHasher.HashPassword(regularUser, "User@123");
            await userRepository.CreateAsync(regularUser);
        }
    }
}