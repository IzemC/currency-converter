using Microsoft.AspNetCore.Identity;
using CurrencyConverterAPI.Services.Interfaces;
using CurrencyConverterAPI.Repositories;
using CurrencyConverterAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


namespace CurrencyConverterAPI.Services;


public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        IConfiguration configuration,
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> AuthenticateAsync(AuthRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or password");

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordVerificationResult != PasswordVerificationResult.Success)
            throw new UnauthorizedAccessException("Invalid username or password");

        // Generate JWT token
        var token = GenerateJwtToken(user);
        var tokenExpires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"]));

        // Generate refresh token
        var refreshToken = GenerateRefreshToken();
        user.AddRefreshToken(refreshToken);
        user.LastLogin = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(user);

        return new AuthResponse(user, token, refreshToken, tokenExpires);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
    {
        var user = await GetUserByTokenAsync(token);
        var existingRefreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken);

        if (user == null || existingRefreshToken == null || !existingRefreshToken.IsActive)
            throw new UnauthorizedAccessException("Invalid token");

        // Replace old refresh token with a new one
        user.RemoveRefreshToken(refreshToken);
        var newRefreshToken = GenerateRefreshToken();
        user.AddRefreshToken(newRefreshToken);
        
        await _userRepository.UpdateAsync(user);

        // Generate new JWT
        var newToken = GenerateJwtToken(user);
        var tokenExpires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"]));

        return new AuthResponse(user, newToken, newRefreshToken, tokenExpires);
    }

    private string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<User> GetUserByTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var userId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;

        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<User> GetUserByIdAsync(string userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }
}