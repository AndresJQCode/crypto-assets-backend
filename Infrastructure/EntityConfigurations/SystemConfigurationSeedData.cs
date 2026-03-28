using Domain.AggregatesModel.SystemConfigurationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityConfigurations;

public static class SystemConfigurationSeedData
{
    public static void SeedSystemConfigurations(ModelBuilder modelBuilder)
    {
        var systemUser = "SYSTEM";
        var seedDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        modelBuilder.Entity<SystemConfiguration>().HasData(
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Key = "OrderProcessing.PubSub.Enabled",
                Value = "true",
                Description = "Enable/disable order processing from Pub/Sub queue. Set to false before deployments to prevent incomplete processing.",
                IsActive = true,
                CreatedOn = seedDate,
                LastModifiedOn = seedDate,
                LastModifiedByName = systemUser
            },
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Key = "OrderProcessing.PubSub.BatchSize",
                Value = "10",
                Description = "Number of messages to pull from Pub/Sub in each batch.",
                IsActive = true,
                CreatedOn = seedDate,
                LastModifiedOn = seedDate,
                LastModifiedByName = systemUser
            },
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Key = "OrderProcessing.PubSub.PollIntervalSeconds",
                Value = "5",
                Description = "Interval in seconds between Pub/Sub pull attempts.",
                IsActive = true,
                CreatedOn = seedDate,
                LastModifiedOn = seedDate,
                LastModifiedByName = systemUser
            }
        );
    }
}
