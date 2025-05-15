using CurrencyConverterAPI.Models;

namespace CurrencyConverterAPI.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string userId);
    Task<User?> GetByUsernameAsync(string username);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
} 