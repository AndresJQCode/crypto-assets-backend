using Domain.AggregatesModel.OrderAggregate;
using Domain.AggregatesModel.TenantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.OrderConfigurations;

public class OrderIntegrationEventConfiguration : IEntityTypeConfiguration<OrderIntegrationEvent>
{
    public void Configure(EntityTypeBuilder<OrderIntegrationEvent> builder)
    {
        builder.ToTable("OrderIntegrationEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.ExternalOrderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Platform)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.EventPayload)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        // Relaciones
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.IdempotencyKey })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(e => new { e.TenantId, e.Platform, e.ExternalOrderId, e.EventTimestamp });
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.ReceivedAt);
    }
}
