using Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.UserConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedNever(); // UUID se genera en código, no en BD

        builder.Property(u => u.Email)
            .IsRequired();

        // Índice único para Username
        builder.HasIndex(u => u.UserName)
            .IsUnique();

        // Índice único para Email
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder
        .HasMany(u => u.UserRoles)
        .WithOne()
        .HasForeignKey(ur => ur.UserId)
        .IsRequired();
    }
}
