using Microsoft.AspNetCore.Identity;

namespace CaptchaGenerator.Models.Entites;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryDate { get; set; }
}
