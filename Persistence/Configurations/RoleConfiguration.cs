
using CaptchaGenerator.Models.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaptchaGenerator.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.NormalizedName).IsRequired();

        builder.HasMany(p => p.UserRoles)
            .WithOne(c => c.Role)
            .HasForeignKey(c => c.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
