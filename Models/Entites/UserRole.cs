using CaptchaGenerator.Models.Abstractions;

namespace CaptchaGenerator.Models.Entites;

public sealed class UserRole : EntityBase
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public User User { get; set; }
    public Role Role { get; set; }

    public UserRole()
    {
        
    }

    public UserRole(Guid id,Guid userId, Guid roleId)
    {
        Id = id;
        UserId = userId;
        RoleId = roleId;
    }
}
