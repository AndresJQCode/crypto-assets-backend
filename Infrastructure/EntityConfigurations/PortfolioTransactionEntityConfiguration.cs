using Domain.AggregatesModel.PortfolioAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations;

public class PortfolioTransactionEntityConfiguration : IEntityTypeConfiguration<PortfolioTransaction>
{
    public void Configure(EntityTypeBuilder<PortfolioTransaction> builder)
    {
        builder.ToTable("PortfolioTransactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.PortfolioId)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(t => t.BalanceAfter)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(t => t.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(t => t.TradingOrderId);

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        // Type as owned entity (Enumeration pattern)
        builder.Property(t => t.Type)
            .HasConversion(
                v => v.Id,
                v => TransactionType.FromValue<TransactionType>(v))
            .IsRequired();

        // Indexes
        builder.HasIndex(t => t.PortfolioId)
            .HasDatabaseName("IX_PortfolioTransactions_PortfolioId");

        builder.HasIndex(t => t.TransactionDate)
            .HasDatabaseName("IX_PortfolioTransactions_TransactionDate");

        builder.HasIndex(t => t.TradingOrderId)
            .HasDatabaseName("IX_PortfolioTransactions_TradingOrderId");

        builder.HasIndex(t => new { t.PortfolioId, t.TransactionDate })
            .HasDatabaseName("IX_PortfolioTransactions_PortfolioId_TransactionDate");
    }
}
