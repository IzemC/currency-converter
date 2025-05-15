using System.ComponentModel.DataAnnotations;

namespace CurrencyConverterAPI.Models; 

public class RefreshToken
{
    public string Token { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime Expires { get; set; } = DateTime.UtcNow.AddDays(7);
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public string CreatedByIp { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsActive => !IsExpired;
}


public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; }
    
    [Required]
    public string RefreshToken { get; set; }
}