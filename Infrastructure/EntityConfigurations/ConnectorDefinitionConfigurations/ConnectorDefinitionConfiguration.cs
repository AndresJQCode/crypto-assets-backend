using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.ConnectorDefinitionConfigurations;

public class ConnectorDefinitionConfiguration : IEntityTypeConfiguration<ConnectorDefinition>
{
    public void Configure(EntityTypeBuilder<ConnectorDefinition> builder)
    {
        builder.ToTable("ConnectorDefinitions");

        // Primary key
        builder.HasKey(cd => cd.Id);
        builder.Property(cd => cd.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(cd => cd.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cd => cd.LogoUrl)
            .HasMaxLength(500);

        builder.Property(cd => cd.ProviderType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cd => cd.CategoryType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cd => cd.Description)
            .HasMaxLength(1000);

        builder.Property(cd => cd.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(cd => cd.RequiresOAuth)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cd => cd.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Audit fields
        builder.Property(cd => cd.CreatedOn)
            .IsRequired();

        builder.Property(cd => cd.LastModifiedOn);
        builder.Property(cd => cd.CreatedBy);
        builder.Property(cd => cd.LastModifiedBy);
        builder.Property(cd => cd.LastModifiedByName)
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(cd => cd.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_ConnectorDefinitions_Name_Unique");

        builder.HasIndex(cd => cd.ProviderType)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_ConnectorDefinitions_ProviderType_Unique");

        builder.HasIndex(cd => cd.IsActive)
            .HasDatabaseName("IX_ConnectorDefinitions_IsActive");
    }
}
