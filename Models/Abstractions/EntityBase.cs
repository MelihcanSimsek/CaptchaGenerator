
namespace CaptchaGenerator.Models.Abstractions;

public abstract class EntityBase : IEquatable<EntityBase>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public bool Equals(EntityBase? other)
    {
        if (other is null) return false;

        if (other is not EntityBase entityBase) return false;

        if (other.GetType() != GetType()) return false;

        return entityBase.Id == Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;

        if (obj is not EntityBase entityBase) return false;

        if (obj.GetType() != GetType()) return false;

        return entityBase.Id == Id;
    }
}
