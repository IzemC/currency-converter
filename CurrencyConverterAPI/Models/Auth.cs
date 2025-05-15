using System.ComponentModel.DataAnnotations;

namespace CurrencyConverterAPI.Models; 

public class AuthRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }


public class AuthResponse
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime TokenExpires { get; set; }

    public AuthResponse(User user, string token, string refreshToken, DateTime tokenExpires)
    {
        Id = user.Id;
        Username = user.Username;
        Role = user.Role;
        Token = token;
        RefreshToken = refreshToken;
        TokenExpires = tokenExpires;
    }
}