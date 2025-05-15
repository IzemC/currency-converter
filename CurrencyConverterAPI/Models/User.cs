namespace CurrencyConverterAPI.Models; 

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    public bool HasValidRefreshToken(string refreshToken) =>
        RefreshTokens.Any(rt => rt.Token == refreshToken && rt.IsActive);

    public void AddRefreshToken(string token)
    {
        RefreshTokens.Add(new RefreshToken
        {
            Token = token,
            Expires = DateTime.UtcNow.AddDays(7)
        });
    }

    public void RemoveRefreshToken(string refreshToken)
    {
        RefreshTokens.RemoveAll(rt => rt.Token == refreshToken);
    }
}

public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }