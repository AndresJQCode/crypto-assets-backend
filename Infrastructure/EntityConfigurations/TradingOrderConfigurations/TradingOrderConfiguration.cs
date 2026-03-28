using Domain.AggregatesModel.TradingOrderAggregate;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations.TradingOrderConfigurations;

public class TradingOrderConfiguration : IEntityTypeConfiguration<TradingOrder>
{
    public void Configure(EntityTypeBuilder<TradingOrder> builder)
    {
        builder.ToTable("TradingOrders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.ExternalOrderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Symbol)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.Side)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.OrderType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>();

        // High precision for crypto amounts (8 decimal places)
        builder.Property(o => o.Quantity)
            .IsRequired()
            .HasPrecision(18, 8);

        builder.Property(o => o.Price)
            .IsRequired()
            .HasPrecision(18, 8);

        builder.Property(o => o.StopPrice)
            .HasPrecision(18, 8);

        builder.Property(o => o.TriggerPrice)
            .HasPrecision(18, 8);

        builder.Property(o => o.FilledQuantity)
            .IsRequired()
            .HasPrecision(18, 8);

        builder.Property(o => o.AveragePrice)
            .HasPrecision(18, 8);

        builder.Property(o => o.Fee)
            .HasPrecision(18, 8);

        builder.Property(o => o.FeeCurrency)
            .HasMaxLength(10);

        builder.Property(o => o.CreatedTime)
            .IsRequired();

        builder.Property(o => o.UpdatedTime);

        builder.Property(o => o.LastSyncedAt)
            .IsRequired();

        builder.Property(o => o.RawData)
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne<ConnectorInstance>()
            .WithMany()
            .HasForeignKey(o => o.ConnectorInstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(o => o.ConnectorInstanceId);
        builder.HasIndex(o => new { o.ConnectorInstanceId, o.ExternalOrderId })
            .IsUnique();
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.Symbol);
        builder.HasIndex(o => o.CreatedTime);
        builder.HasIndex(o => o.LastSyncedAt);
    }
}
