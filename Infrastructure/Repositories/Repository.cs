using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;
using Domain.SeedWork;
using Infrastructure.Constants;
using Infrastructure.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class Repository<T>(
    ApiContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<Repository<T>> logger)
    : IRepository<T> where T : Entity<Guid>, IAggregateRoot
{
    private static readonly string EntityName = typeof(T).Name;
    private readonly Guid _currentUserId = GetCurrentUserId(httpContextAccessor);

    public IUnitOfWork UnitOfWork => (IUnitOfWork)context;

    #region Metrics and Logging Helper Methods

    private void RecordSuccess(string operation, TimeSpan elapsed)
    {
        InfrastructureMetrics.DatabaseQueriesTotal
            .WithLabels(operation, EntityName, MetricsLabelsConstants.Database.Success)
            .Inc();

        InfrastructureMetrics.DatabaseQueryDuration
            .WithLabels(operation, EntityName)
            .Observe(elapsed.TotalSeconds);

        logger.LogDebug(
            "Repository operation {Operation} on {EntityType} completed successfully in {ElapsedMs}ms",
            operation, EntityName, elapsed.TotalMilliseconds);
    }

    private void RecordError(string operation, Exception exception)
    {
        InfrastructureMetrics.DatabaseQueriesTotal
            .WithLabels(operation, EntityName, MetricsLabelsConstants.Database.Error)
            .Inc();

        InfrastructureMetrics.DatabaseErrorsTotal
            .WithLabels($"{operation}_error", EntityName)
            .Inc();

        logger.LogError(exception,
            "Repository operation {Operation} on {EntityType} failed",
            operation, EntityName);
    }

    private async Task<TResult> ExecuteWithMetricsAsync<TResult>(
        string operation,
        Func<Task<TResult>> action)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            logger.LogDebug(
                "Starting repository operation {Operation} on {EntityType}",
                operation, EntityName);

            var result = await action();
            stopwatch.Stop();
            RecordSuccess(operation, stopwatch.Elapsed);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            RecordError(operation, ex);
            throw;
        }
    }

    private TResult ExecuteWithMetrics<TResult>(
        string operation,
        Func<TResult> action)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            logger.LogDebug(
                "Starting repository operation {Operation} on {EntityType}",
                operation, EntityName);

            var result = action();
            stopwatch.Stop();
            RecordSuccess(operation, stopwatch.Elapsed);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            RecordError(operation, ex);
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private static Guid GetCurrentUserId(IHttpContextAccessor accessor)
    {
        try
        {
            var userIdClaim = accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userIdClaim) ? Guid.Empty : Guid.Parse(userIdClaim);
        }
        catch
        {
            return Guid.Empty;
        }
    }

    /// <summary>
    /// Aplica includes type-safe usando expresiones lambda
    /// </summary>
    private static IQueryable<T> ApplyIncludes(IQueryable<T> query, Expression<Func<T, object>>[] includes)
    {
        if (includes.Length == 0)
            return query;

        return includes.Aggregate(query, (current, include) => current.Include(include));
    }

    private void SetAuditFields(T entity, bool isUpdate = false)
    {
        var now = DateTimeOffset.UtcNow;

        if (!isUpdate)
        {
            entity.CreatedBy = _currentUserId;
            entity.CreatedOn = now;
        }

        entity.LastModifiedBy = _currentUserId;
        entity.LastModifiedOn = now;
    }

    private void SetAuditFields(IReadOnlyCollection<T> entities, bool isUpdate = false)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entity in entities)
        {
            if (!isUpdate)
            {
                entity.CreatedBy = _currentUserId;
                entity.CreatedOn = now;
            }

            entity.LastModifiedBy = _currentUserId;
            entity.LastModifiedOn = now;
        }
    }

    private DbSet<T> GetQueryable()
    {
        return context.Set<T>();
    }

    /// <summary>
    /// Aplica una especificación a un query
    /// </summary>
    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        IQueryable<T> query = GetQueryable();

        // Tracking
        if (!spec.AsTracking)
            query = query.AsNoTracking();

        // Includes
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Criteria
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);

        // Ordering
        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending != null)
            query = query.OrderByDescending(spec.OrderByDescending);

        return query;
    }

    #endregion

    #region CRUD Methods

    public Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return GetQueryable().AnyAsync(predicate, cancellationToken);
    }

    public async Task<T> Create(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.Insert, async () =>
        {
            logger.LogDebug(
                "Creating entity {EntityType} with ID {EntityId}",
                EntityName, entity.Id);

            SetAuditFields(entity);
            await context.Set<T>().AddAsync(entity, cancellationToken);
            return entity;
        });
    }

    public async Task<List<T>> CreateRange(IReadOnlyCollection<T> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count == 0)
            return [];

        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.InsertRange, async () =>
        {
            logger.LogDebug(
                "Creating {Count} entities of type {EntityType}",
                entities.Count, EntityName);

            var entitiesList = entities.ToList();
            SetAuditFields(entitiesList);
            await context.Set<T>().AddRangeAsync(entitiesList, cancellationToken);
            return entitiesList;
        });
    }

    public T Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return ExecuteWithMetrics(MetricsLabelsConstants.Database.Update, () =>
        {
            logger.LogDebug(
                "Updating entity {EntityType} with ID {EntityId}",
                EntityName, entity.Id);

            SetAuditFields(entity, isUpdate: true);
            context.Entry(entity).State = EntityState.Modified;
            context.Set<T>().Update(entity);
            return entity;
        });
    }

    public void UpdateRange(IReadOnlyCollection<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count == 0)
            return;

        ExecuteWithMetrics("update_range", () =>
        {
            logger.LogDebug(
                "Updating {Count} entities of type {EntityType}",
                entities.Count, EntityName);

            var entitiesList = entities.ToList();
            SetAuditFields(entitiesList, isUpdate: true);
            context.Set<T>().UpdateRange(entitiesList);
            return true;
        });
    }

    #endregion

    #region Delete Methods

    /// <summary>
    /// Elimina la entidad físicamente de la base de datos (hard delete).
    /// El repositorio no ejecuta lógica de dominio; solo persiste la eliminación.
    /// </summary>
    public bool Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return ExecuteWithMetrics(MetricsLabelsConstants.Database.Delete, () =>
        {
            logger.LogDebug(
                "Deleting entity {EntityType} with ID {EntityId}",
                EntityName, entity.Id);

            var removedEntity = context.Set<T>().Remove(entity);
            return removedEntity != null;
        });
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Ejecuta eliminación masiva directamente en la base de datos sin cargar entidades.
    /// Usa ExecuteDelete de EF Core 7+ para mejor rendimiento.
    /// ADVERTENCIA: Esta operación es física (hard delete) y bypasea el método Delete() de la entidad.
    /// </summary>
    /// <param name="predicate">Filtro para las entidades a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de filas afectadas</returns>
    /// <example>
    /// // Eliminar todos los usuarios inactivos
    /// int deleted = await repo.ExecuteDeleteAsync(u => !u.IsActive);
    /// </example>
    public async Task<int> ExecuteDeleteAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.BulkDelete, async () =>
        {
            logger.LogInformation(
                "Executing bulk delete on {EntityType}",
                EntityName);

            var affected = await GetQueryable()
                .Where(predicate)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Bulk delete on {EntityType} affected {AffectedRows} rows",
                EntityName, affected);

            return affected;
        });
    }

    // NOTA: ExecuteUpdateAsync no está disponible como método wrapper en el repositorio debido a que
    // SetPropertyCalls<T> no es un tipo público en EF Core 10. Para usar ExecuteUpdateAsync,
    // accede directamente al DbContext en tus handlers:
    //
    // await context.Set<Product>()
    //     .Where(p => p.Category == "Electronics")
    //     .ExecuteUpdateAsync(s => {
    //         s.SetProperty(p => p.Price, p => p.Price * 1.1m);
    //         s.SetProperty(p => p.LastModifiedOn, DateTimeOffset.UtcNow);
    //     });
    //
    // Referencia: https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete

    #endregion

    #region Specification Pattern Methods

    /// <summary>
    /// Obtiene entidades usando una especificación reutilizable
    /// </summary>
    /// <param name="specification">Especificación que encapsula la query</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de entidades que cumplen la especificación</returns>
    /// <example>
    /// // Crear especificación reutilizable
    /// var spec = new ActiveUsersWithRolesSpec();
    /// var users = await repo.GetBySpecification(spec);
    /// </example>
    public async Task<List<T>> GetBySpecification(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.SpecificationSelect, async () =>
        {
            logger.LogDebug(
                "Executing specification {SpecificationType} on {EntityType}",
                specification.GetType().Name, EntityName);

            var query = ApplySpecification(specification);

            // Aplicar paginación si está especificada (pero sin retornar PagedResult)
            if (specification.Page.HasValue)
            {
                var skip = (specification.Page.Value - 1) * specification.PageSize;
                query = query.Skip(skip).Take(specification.PageSize);
            }

            return await query.ToListAsync(cancellationToken);
        });
    }

    /// <summary>
    /// Obtiene la primera entidad que cumpla la especificación
    /// </summary>
    /// <param name="specification">Especificación que encapsula la query</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Primera entidad o null</returns>
    public async Task<T?> GetFirstBySpecification(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.SpecificationSelectFirst, async () =>
        {
            logger.LogDebug(
                "Executing specification {SpecificationType} (first) on {EntityType}",
                specification.GetType().Name, EntityName);

            var query = ApplySpecification(specification);
            return await query.FirstOrDefaultAsync(cancellationToken);
        });
    }

    /// <summary>
    /// Obtiene el conteo de entidades que cumplen la especificación
    /// </summary>
    /// <param name="specification">Especificación que encapsula la query</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de entidades</returns>
    public async Task<int> GetCountBySpecification(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.SpecificationCount, async () =>
        {
            logger.LogDebug(
                "Counting with specification {SpecificationType} on {EntityType}",
                specification.GetType().Name, EntityName);

            var query = ApplySpecification(specification);
            return await query.CountAsync(cancellationToken);
        });
    }

    /// <summary>
    /// Obtiene entidades paginadas usando una especificación
    /// </summary>
    /// <param name="specification">Especificación que debe incluir paginación (Page y PageSize)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado</returns>
    /// <exception cref="InvalidOperationException">Si la especificación no tiene paginación configurada</exception>
    public async Task<PagedResult<T>> GetBySpecificationPaginated(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        if (!specification.Page.HasValue)
            throw new InvalidOperationException(
                "La especificación debe tener paginación configurada (Page y PageSize)");

        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.SpecificationPaginated, async () =>
        {
            logger.LogDebug(
                "Executing paginated specification {SpecificationType} on {EntityType} (Page: {Page}, Size: {PageSize})",
                specification.GetType().Name, EntityName, specification.Page, specification.PageSize);

            var query = ApplySpecification(specification);

            var totalCount = await query.CountAsync(cancellationToken);

            var skip = (specification.Page.Value - 1) * specification.PageSize;
            query = query.Skip(skip).Take(specification.PageSize);

            var items = await query.ToListAsync(cancellationToken);

            return new PagedResult<T>(totalCount, items, specification.Page.Value, specification.PageSize);
        });
    }

    #endregion

    #region Query Methods

    public async Task<List<T>> Query(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool tracking = false,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.Query, async () =>
        {
            IQueryable<T> query = GetQueryable();

            if (!tracking)
                query = query.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync(cancellationToken);
        });
    }

    /// <summary>
    /// Proyecta entidades a un tipo diferente sin cargar la entidad completa.
    /// Mejora el rendimiento al seleccionar solo los campos necesarios.
    /// </summary>
    /// <typeparam name="TResult">Tipo del resultado de la proyección</typeparam>
    /// <param name="selector">Expresión de proyección</param>
    /// <param name="filter">Filtro opcional</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de resultados proyectados</returns>
    /// <example>
    /// var userNames = await repo.Select(
    ///     selector: u => new { u.Id, u.Name, u.Email },
    ///     filter: u => u.IsActive);
    /// </example>
    public async Task<List<TResult>> Select<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selector);

        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.SelectProjection, async () =>
        {
            IQueryable<T> query = GetQueryable().AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            return await query.Select(selector).ToListAsync(cancellationToken);
        });
    }

    /// <summary>
    /// Proyecta la primera entidad que coincida con el filtro a un tipo diferente.
    /// </summary>
    /// <typeparam name="TResult">Tipo del resultado de la proyección</typeparam>
    /// <param name="selector">Expresión de proyección</param>
    /// <param name="filter">Filtro opcional</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado proyectado o null si no se encuentra</returns>
    /// <example>
    /// var userName = await repo.SelectFirstOrDefault(
    ///     selector: u => new { u.Id, u.Name },
    ///     filter: u => u.Email == email);
    /// </example>
    public async Task<TResult?> SelectFirstOrDefault<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selector);

        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.SelectFirstProjection, async () =>
        {
            IQueryable<T> query = GetQueryable().AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            return await query.Select(selector).FirstOrDefaultAsync(cancellationToken);
        });
    }

    /// <summary>
    /// Retorna un IAsyncEnumerable para procesar entidades una por una sin cargar todo en memoria.
    /// Ideal para datasets grandes donde ToListAsync() consumiría demasiada memoria.
    /// </summary>
    /// <param name="filter">Filtro opcional</param>
    /// <param name="tracking">Si debe hacer tracking de cambios (default: false)</param>
    /// <param name="includes">Propiedades de navegación a incluir</param>
    /// <returns>IAsyncEnumerable para iterar con await foreach</returns>
    /// <example>
    /// await foreach (var user in repo.AsAsyncEnumerable(u => u.IsActive))
    /// {
    ///     await ProcessUserAsync(user);
    /// }
    /// </example>
    public IAsyncEnumerable<T> AsAsyncEnumerable(
        Expression<Func<T, bool>>? filter = null,
        bool tracking = false,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = GetQueryable();

        if (!tracking)
            query = query.AsNoTracking();

        query = ApplyIncludes(query, includes);

        if (filter != null)
            query = query.Where(filter);

        return query.AsAsyncEnumerable();
    }

    /// <summary>
    /// Obtiene entidades por filtro con includes type-safe y ordenamiento
    /// </summary>
    /// <example>
    /// var users = await repo.GetByFilter(
    ///     filter: u => u.IsActive,
    ///     orderBy: q => q.OrderBy(u => u.Name),
    ///     includes: u => u.Roles, u => u.Permissions);
    /// </example>
    public async Task<List<T>> GetByFilter(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool tracking = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.Select, async () =>
        {
            IQueryable<T> query = GetQueryable();

            if (!tracking)
                query = query.AsNoTracking();

            query = ApplyIncludes(query, includes);

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync(cancellationToken);
        });
    }

    public async Task<PagedResult<T>> GetByFilterPagination(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int page = 1,
        int pageSize = 10,
        bool includeCount = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        if (page < 1)
            throw new ArgumentException("Page debe ser mayor a 0", nameof(page));

        if (pageSize < 1)
            throw new ArgumentException("Page size debe ser mayor a 0", nameof(pageSize));

        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.SelectPaginated, async () =>
        {
            IQueryable<T> query = GetQueryable().AsNoTracking();

            query = ApplyIncludes(query, includes);

            if (filter != null)
                query = query.Where(filter);

            var totalCount = 0;
            if (includeCount)
            {
                totalCount = await query.CountAsync(cancellationToken);
            }

            if (orderBy != null)
                query = orderBy(query);

            var skip = (page - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);

            // Materializar el query antes de retornar para evitar errores de DbContext disposed
            var items = await query.ToListAsync(cancellationToken);

            return new PagedResult<T>(totalCount, items, page, pageSize);
        });
    }

    public async Task<int> GetCountByFilter(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<T> query = GetQueryable();
        query = query.Where(filter);
        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene la primera entidad que coincida con el filtro
    /// </summary>
    /// <example>
    /// var user = await repo.GetFirstOrDefaultByFilter(
    ///     filter: u => u.Email == email,
    ///     includes: u => u.Roles);
    /// </example>
    public async Task<T?> GetFirstOrDefaultByFilter(
        Expression<Func<T, bool>>? filter = null,
        bool tracking = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.SelectFirst, async () =>
        {
            IQueryable<T> query = GetQueryable();

            if (!tracking)
                query = query.AsNoTracking();

            query = ApplyIncludes(query, includes);

            if (filter != null)
                query = query.Where(filter);

            return await query.FirstOrDefaultAsync(cancellationToken);
        });
    }

    /// <summary>
    /// Obtiene una entidad por su ID con includes type-safe
    /// </summary>
    /// <example>
    /// var user = await repo.GetById(userId, includes: u => u.Roles, u => u.Permissions);
    /// </example>
    public async Task<T?> GetById(
        Guid id,
        bool tracking = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        return await ExecuteWithMetricsAsync(MetricsLabelsConstants.Database.SelectById, async () =>
        {
            IQueryable<T> query = GetQueryable();

            if (!tracking)
                query = query.AsNoTracking();

            query = ApplyIncludes(query, includes);

            return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        });
    }

    #endregion
}
