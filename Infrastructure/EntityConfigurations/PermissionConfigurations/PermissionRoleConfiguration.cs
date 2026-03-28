using Domain.AggregatesModel.PermissionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.PermissionConfigurations
{
    public class PermissionRoleConfiguration : IEntityTypeConfiguration<PermissionRole>
    {
        public void Configure(EntityTypeBuilder<PermissionRole> builder)
        {
            builder.ToTable("PermissionRoles");

            builder.HasKey(pr => pr.Id);

            builder.Property(pr => pr.Id)
                .ValueGeneratedNever(); // UUID se genera en código, no en BD

            builder.Property(pr => pr.PermissionId)
                .IsRequired();

            builder.Property(pr => pr.RoleId)
                .IsRequired();

            // Las propiedades AssignedOn y AssignedBy ahora se mapean a CreatedOn y CreatedBy de Entity
            builder.Property(pr => pr.CreatedOn)
                .IsRequired();

            builder.Property(pr => pr.CreatedBy);

            builder.Property(pr => pr.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices
            builder.HasIndex(pr => new { pr.PermissionId, pr.RoleId, pr.IsActive })
                .IsUnique();

            builder.HasIndex(pr => pr.PermissionId);

            builder.HasIndex(pr => pr.RoleId);

            builder.HasIndex(pr => pr.IsActive);

            // Relaciones
            builder.HasOne(pr => pr.Permission)
                .WithMany(p => p.PermissionRoles)
                .HasForeignKey(pr => pr.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pr => pr.Role)
                .WithMany(r => r.PermissionRoles)
                .HasForeignKey(pr => pr.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
