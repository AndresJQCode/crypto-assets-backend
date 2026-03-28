using Domain.AggregatesModel.RoleAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.UserConfigurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .ValueGeneratedNever(); // UUID se genera en código, no en BD

        // Configuración de la propiedad Description
        builder.Property(r => r.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder
        .HasMany(r => r.UserRoles)
        .WithOne()
        .HasForeignKey(ur => ur.RoleId)
        .IsRequired();

        builder
        .HasMany(r => r.PermissionRoles)
        .WithOne(pr => pr.Role)
        .HasForeignKey(pr => pr.RoleId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
