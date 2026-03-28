using Domain.AggregatesModel.PermissionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.PermissionConfigurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedNever(); // UUID se genera en código, no en BD

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.Resource)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Action)
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(p => p.CreatedOn)
                .IsRequired();

            builder.Property(p => p.LastModifiedOn);

            builder.Property(p => p.CreatedBy);

            builder.Property(p => p.LastModifiedBy);

            // Índices
            builder.HasIndex(p => p.Name)
                .IsUnique();

            builder.HasIndex(p => new { p.Resource, p.Action })
                .IsUnique();


            // Relaciones
            builder.HasMany(p => p.PermissionRoles)
                .WithOne(pr => pr.Permission)
                .HasForeignKey(pr => pr.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
