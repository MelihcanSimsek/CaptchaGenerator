using CaptchaGenerator.Models.Abstractions;

namespace CaptchaGenerator.Models.Entites;

public sealed class Role : EntityBase
{
    public string Name { get; set; }
    public string NormalizedName { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }

    public Role()
    {
        
    }

    public Role(Guid id, string name, string normalizedName)
    {
        Id = id;
        Name = name;
        NormalizedName = normalizedName;
    }
}