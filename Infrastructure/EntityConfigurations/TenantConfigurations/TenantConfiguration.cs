using Domain.AggregatesModel.TenantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.TenantConfigurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(200);
        builder.Property(t => t.CountryName).IsRequired(false).HasMaxLength(100);
        builder.Property(t => t.CountryPhoneCode).IsRequired(false).HasMaxLength(10);
        builder.Property(t => t.WhatsAppNumber).IsRequired(false).HasMaxLength(20);
        builder.Property(t => t.IsActive).IsRequired();

        builder.HasIndex(t => t.Slug).IsUnique();
    }
}
