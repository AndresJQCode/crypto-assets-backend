using Domain.AggregatesModel.AuditAggregate;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.SeedWork;
using Infrastructure.EntityConfigurations.AuditConfigurations;
using Infrastructure.EntityConfigurations.PermissionConfigurations;
using Infrastructure.EntityConfigurations.UserConfigurations;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Linq;

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


    public new DbSet<User> Users { get; set; }
    public new DbSet<Role> Roles { get; set; }
    public new DbSet<UserClaim> UserClaims { get; set; }
    public new DbSet<UserRole> UserRoles { get; set; }
    public new DbSet<UserLogin> UserLogins { get; set; }
    public new DbSet<RoleClaim> RoleClaims { get; set; }
    public new DbSet<UserToken> UserTokens { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<PermissionRole> PermissionRoles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

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

    public async System.Threading.Tasks.Task CommitTransactionAsync(IDbContextTransaction transaction)
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
        public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield break;
        }

        public async IAsyncEnumerable<object> CreateStream(object request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield break;
        }

        public System.Threading.Tasks.Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
#pragma warning disable CS8604 // Posible argumento de referencia nulo
            return System.Threading.Tasks.Task.FromResult<TResponse>(default);
#pragma warning restore CS8604 // Posible argumento de referencia nulo
        }

        public System.Threading.Tasks.Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(default(object));
        }

        public System.Threading.Tasks.Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            return System.Threading.Tasks.Task.CompletedTask;
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
