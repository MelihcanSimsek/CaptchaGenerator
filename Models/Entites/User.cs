
using CaptchaGenerator.Models.Abstractions;

namespace CaptchaGenerator.Models.Entites;

public sealed class User : EntityBase
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryDate { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
    public User()
    {
        
    }

    public User(Guid id,string fullName, string email, byte[] passwordHash, byte[] passwordSalt, string? refreshToken, DateTime? refreshTokenExpiryDate)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        RefreshToken = refreshToken;
        RefreshTokenExpiryDate = refreshTokenExpiryDate;
    }
}
