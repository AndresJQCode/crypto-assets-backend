using Domain.AggregatesModel.CustomerAggregate;
using Domain.AggregatesModel.OrderAggregate;
using Domain.AggregatesModel.TenantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.OrderConfigurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.ExternalOrderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Platform)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.PaymentStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.FulfillmentStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.Total)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Subtotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Tax)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.ShippingCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Discount)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(o => o.PlatformMetadata)
            .HasColumnType("jsonb");

        // Value Objects
        builder.OwnsOne(o => o.ShippingAddress, sa =>
        {
            sa.Property(a => a.FirstName).HasMaxLength(100).IsRequired();
            sa.Property(a => a.LastName).HasMaxLength(100).IsRequired();
            sa.Property(a => a.Address1).HasMaxLength(255).IsRequired();
            sa.Property(a => a.Address2).HasMaxLength(255);
            sa.Property(a => a.City).HasMaxLength(100).IsRequired();
            sa.Property(a => a.Province).HasMaxLength(100);
            sa.Property(a => a.Country).HasMaxLength(100).IsRequired();
            sa.Property(a => a.PostalCode).HasMaxLength(20);
            sa.Property(a => a.Phone).HasMaxLength(50);
        });

        builder.OwnsOne(o => o.BillingAddress, ba =>
        {
            ba.Property(a => a.FirstName).HasMaxLength(100).IsRequired();
            ba.Property(a => a.LastName).HasMaxLength(100).IsRequired();
            ba.Property(a => a.Address1).HasMaxLength(255).IsRequired();
            ba.Property(a => a.Address2).HasMaxLength(255);
            ba.Property(a => a.City).HasMaxLength(100).IsRequired();
            ba.Property(a => a.Province).HasMaxLength(100);
            ba.Property(a => a.Country).HasMaxLength(100).IsRequired();
            ba.Property(a => a.PostalCode).HasMaxLength(20);
            ba.Property(a => a.Phone).HasMaxLength(50);
        });

        // Relaciones
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(o => o.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(o => o.TenantId);
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.OrderNumber);
        builder.HasIndex(o => new { o.TenantId, o.Platform, o.ExternalOrderId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.OrderDate);
        builder.HasIndex(o => o.LastSyncedAt);
    }
}
