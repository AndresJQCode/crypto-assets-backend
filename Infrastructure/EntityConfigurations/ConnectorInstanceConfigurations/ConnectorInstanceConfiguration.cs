using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.AggregatesModel.TenantAggregate;
using Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.ConnectorInstanceConfigurations;

public class ConnectorInstanceConfiguration : IEntityTypeConfiguration<ConnectorInstance>
{
    public void Configure(EntityTypeBuilder<ConnectorInstance> builder)
    {
        builder.ToTable("ConnectorInstances");

        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedNever(); // Generated in domain with Guid.CreateVersion7()

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Enums stored as integers
        builder.Property(c => c.ProviderType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.CategoryType)
            .IsRequired()
            .HasConversion<int>();

        // JSONB configuration column (PostgreSQL)
        builder.Property(c => c.ConfigurationJson)
            .IsRequired()
            .HasColumnType("jsonb");

        // Access token (encrypted, can be large)
        builder.Property(c => c.AccessToken)
            .HasMaxLength(2000);

        // Boolean flags
        builder.Property(c => c.IsEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsConfigured)
            .IsRequired()
            .HasDefaultValue(false);

        // Timestamps
        builder.Property(c => c.LastSyncedAt)
            .IsRequired(false);

        // Audit fields (inherited from Entity<T>)
        builder.Property(c => c.CreatedOn)
            .IsRequired();

        builder.Property(c => c.LastModifiedOn)
            .IsRequired(false);

        builder.Property(c => c.CreatedBy)
            .IsRequired(false);

        builder.Property(c => c.LastModifiedBy)
            .IsRequired(false);

        builder.Property(c => c.LastModifiedByName)
            .HasMaxLength(255);

        // Soft delete (inherited from Entity<T>)
        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne<ConnectorDefinition>()
            .WithMany()
            .HasForeignKey(c => c.ConnectorDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("IX_ConnectorInstances_TenantId");

        builder.HasIndex(c => c.UserId)
            .HasDatabaseName("IX_ConnectorInstances_UserId");

        builder.HasIndex(c => c.ProviderType)
            .HasDatabaseName("IX_ConnectorInstances_ProviderType");

        builder.HasIndex(c => new { c.TenantId, c.ProviderType, c.IsEnabled })
            .HasDatabaseName("IX_ConnectorInstances_TenantId_ProviderType_IsEnabled");

        // Unique constraint: one connector instance per user + provider type
        builder.HasIndex(c => new { c.UserId, c.ProviderType })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_ConnectorInstances_UserId_ProviderType_Unique");

        // JSONB GIN index for efficient querying
        builder.HasIndex(c => c.ConfigurationJson)
            .HasDatabaseName("IX_ConnectorInstances_ConfigurationJson")
            .HasMethod("gin");
    }
}
