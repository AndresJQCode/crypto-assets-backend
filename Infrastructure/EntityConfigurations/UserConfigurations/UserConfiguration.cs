using Domain.AggregatesModel.TenantAggregate;
using Domain.AggregatesModel.UserAggregate;
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

        builder.Property(u => u.WhatsAppNumber)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(u => u.TenantId);
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // UserName == Email; únicos a nivel global (un usuario por email en todo el sistema)
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder
        .HasMany(u => u.UserRoles)
        .WithOne()
        .HasForeignKey(ur => ur.UserId)
        .IsRequired();
    }
}
