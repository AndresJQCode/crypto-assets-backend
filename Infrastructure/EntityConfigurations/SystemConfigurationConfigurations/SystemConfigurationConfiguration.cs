using Domain.AggregatesModel.SystemConfigurationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.SystemConfigurationConfigurations;

public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("SystemConfigurations");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sc => sc.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(sc => sc.Description)
            .HasMaxLength(1000);

        builder.Property(sc => sc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(sc => sc.Key)
            .IsUnique()
            .HasDatabaseName("IX_SystemConfigurations_Key");

        builder.HasIndex(sc => sc.IsActive)
            .HasDatabaseName("IX_SystemConfigurations_IsActive");

        // Ignore base Entity properties handled by convention
        builder.Ignore(sc => sc.DomainEvents);
    }
}
