using System.Linq;
using Domain.AggregatesModel.AuditAggregate;
using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.AggregatesModel.OrderAggregate;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.SystemConfigurationAggregate;
using Domain.AggregatesModel.TenantAggregate;
using Domain.AggregatesModel.TradingOrderAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.SeedWork;
using Infrastructure.EntityConfigurations;
using Infrastructure.EntityConfigurations.AuditConfigurations;
using Infrastructure.EntityConfigurations.ConnectorDefinitionConfigurations;
using Infrastructure.EntityConfigurations.ConnectorInstanceConfigurations;
using Infrastructure.EntityConfigurations.OrderConfigurations;
using Infrastructure.EntityConfigurations.PermissionConfigurations;
using Infrastructure.EntityConfigurations.SystemConfigurationConfigurations;
using Infrastructure.EntityConfigurations.TenantConfigurations;
using Infrastructure.EntityConfigurations.TradingOrderConfigurations;
using Infrastructure.EntityConfigurations.UserConfigurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class ApiContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>, IUnitOfWork
{
    public const string DefaultSchema = "";

    private readonly IMediator _mediator;
    private IDbContextTransaction? _currentTransaction;

    public ApiContext(DbContextOptions<ApiContext> options) : base(options)
    {
        _mediator = new NoMediator();
    }

    public ApiContext(DbContextOptions<ApiContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));


        System.Diagnostics.Debug.WriteLine("Context::ctor ->" + this.GetHashCode());
    }

    public IDbContextTransaction? CurrentTransaction => _currentTransaction;

    public bool HasActiveTransaction => _currentTransaction != null;


    public new DbSet<User> Users { get; set; } = null!;
    public new DbSet<Role> Roles { get; set; } = null!;
    public new DbSet<UserClaim> UserClaims { get; set; } = null!;
    public new DbSet<UserRole> UserRoles { get; set; } = null!;
    public new DbSet<UserLogin> UserLogins { get; set; } = null!;
    public new DbSet<RoleClaim> RoleClaims { get; set; } = null!;
    public new DbSet<UserToken> UserTokens { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<PermissionRole> PermissionRoles { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<ConnectorDefinition> ConnectorDefinitions { get; set; } = null!;
    public DbSet<ConnectorInstance> ConnectorInstances { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderIntegrationEvent> OrderIntegrationEvents { get; set; } = null!;
    public DbSet<TradingOrder> TradingOrders { get; set; } = null!;
    public DbSet<SystemConfiguration> SystemConfigurations { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new RoleConfiguration());
        builder.ApplyConfiguration(new RoleClaimConfiguration());
        builder.ApplyConfiguration(new UserRoleConfiguration());
        builder.ApplyConfiguration(new UserClaimConfiguration());
        builder.ApplyConfiguration(new UserLoginConfiguration());
        builder.ApplyConfiguration(new UserTokenConfiguration());
        builder.ApplyConfiguration(new PermissionConfiguration());
        builder.ApplyConfiguration(new PermissionRoleConfiguration());
        builder.ApplyConfiguration(new AuditLogConfiguration());
        builder.ApplyConfiguration(new TenantConfiguration());
        builder.ApplyConfiguration(new ConnectorDefinitionConfiguration());
        builder.ApplyConfiguration(new ConnectorInstanceConfiguration());
        builder.ApplyConfiguration(new OrderConfiguration());
        builder.ApplyConfiguration(new OrderItemConfiguration());
        builder.ApplyConfiguration(new OrderIntegrationEventConfiguration());
        builder.ApplyConfiguration(new TradingOrderConfiguration());
        builder.ApplyConfiguration(new SystemConfigurationConfiguration());

        // Seed data
        SystemConfigurationSeedData.SeedSystemConfigurations(builder);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch Domain Events collection. 
        // Choices:
        // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
        // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
        // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
        // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
        await _mediator.DispatchDomainEventsAsync(this);

        // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
        // performed through the DbContext will be committed
        var result = await base.SaveChangesAsync(cancellationToken);

        return true;
    }


    public async Task<IDbContextTransaction?> BeginTransactionAsync()
    {
        if (_currentTransaction != null)
            return null;

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        if (transaction != _currentTransaction)
            throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
}

internal sealed class NoMediator : IMediator
{
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<TResponse>();
    }

    public IAsyncEnumerable<object> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<object>();
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return Task.CompletedTask;
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TResponse>(default!);
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(default(object?));
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        return Task.CompletedTask;
    }
}

public class ContextDesignFactory : IDesignTimeDbContextFactory<ApiContext>
{
    public ApiContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApiContext>()
           .UseNpgsql(configuration.GetConnectionString("DB"));

        return new ApiContext(optionsBuilder.Options, new NoMediator());
    }
}
