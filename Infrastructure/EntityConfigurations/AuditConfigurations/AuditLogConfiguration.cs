using Domain.AggregatesModel.AuditAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.AuditConfigurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.EntityId)
            .IsRequired();

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UserId)
            .IsRequired(false);

        builder.Property(x => x.UserName)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.AdditionalData)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(x => x.Ip)
            .HasMaxLength(45)
            .IsRequired(false);

        // Configurar índices para mejorar el rendimiento de consultas
        builder.HasIndex(x => new { x.EntityType, x.EntityId })
            .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");

        builder.HasIndex(x => x.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");

        builder.HasIndex(x => x.Action)
            .HasDatabaseName("IX_AuditLogs_Action");

        // Configurar campos heredados de Entity
        builder.Property(x => x.CreatedOn)
            .IsRequired();

        builder.Property(x => x.LastModifiedOn)
            .IsRequired(false);

        builder.Property(x => x.CreatedBy)
            .IsRequired(false);

        builder.Property(x => x.LastModifiedBy)
            .IsRequired(false);

        builder.Property(x => x.LastModifiedByName)
            .HasMaxLength(256)
            .IsRequired(false);
    }
}
