using CaptchaGenerator.Models.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaptchaGenerator.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Email).IsRequired();
        builder.Property(p => p.FullName).IsRequired();
        builder.Property(p => p.FullName).IsRequired();

        builder.HasMany(p => p.UserRoles)
              .WithOne(c => c.User)
              .HasForeignKey(c=>c.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
