using CurrencyConverterAPI.Models;

namespace CurrencyConverterAPI.Services.Interfaces;

 public interface IAuthService
    {
        Task<AuthResponse> AuthenticateAsync(AuthRequest request);
        Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken);
        Task<User> GetUserByIdAsync(string userId);
    }