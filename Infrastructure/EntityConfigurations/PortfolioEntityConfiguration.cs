using Domain.AggregatesModel.PortfolioAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigurations;

public class PortfolioEntityConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("Portfolios");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.InitialCapital)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.CurrentBalance)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.TotalDeposits)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.TotalWithdrawals)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.TotalTradingProfit)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.TotalTradingLoss)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.TotalFees)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.LastUpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Portfolios_UserId");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_Portfolios_IsActive");

        // Relationships
        builder.HasMany<PortfolioTransaction>()
            .WithOne(t => t.Portfolio)
            .HasForeignKey(t => t.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore navigation property (EF Core tracks it via HasMany)
        builder.Ignore(p => p.Transactions);
    }
}
