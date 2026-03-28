using Domain.AggregatesModel.OrderAggregate;
using Domain.AggregatesModel.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.OrderConfigurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.ExternalProductId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(oi => oi.VariantName)
            .HasMaxLength(255);

        builder.Property(oi => oi.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(oi => oi.ImageUrl)
            .HasMaxLength(500);

        builder.Ignore(oi => oi.Subtotal);

        // Relación opcional con Product (si existe en el sistema)
        // Comentado porque Product no existe aún
        // builder.HasOne<Product>()
        //     .WithMany()
        //     .HasForeignKey(oi => oi.ProductId)
        //     .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ProductId);
    }
}
