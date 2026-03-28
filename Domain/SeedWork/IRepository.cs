using System.Linq.Expressions;

namespace Domain.SeedWork;

public interface IRepository<T> where T : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }

    // Métodos básicos CRUD
    Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> Create(T entity, CancellationToken cancellationToken = default);
    Task<List<T>> CreateRange(IReadOnlyCollection<T> entities, CancellationToken cancellationToken = default);
    T Update(T entity);
    void UpdateRange(IReadOnlyCollection<T> entities);

    // Eliminación física (hard delete)
    bool Delete(T entity);

    // Métodos de consulta básicos (includes type-safe con Expression)
    Task<T?> GetFirstOrDefaultByFilter(
        Expression<Func<T, bool>>? filter = null,
        bool tracking = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<List<T>> GetByFilter(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool tracking = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<PagedResult<T>> GetByFilterPagination(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int page = 1,
        int pageSize = 10,
        bool includeCount = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<int> GetCountByFilter(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

    Task<T?> GetById(
        Guid id,
        bool tracking = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<List<T>> Query(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool tracking = false,
        CancellationToken cancellationToken = default);

    // Métodos de proyección (Select) - evita cargar entidades completas
    Task<List<TResult>> Select<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    Task<TResult?> SelectFirstOrDefault<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    // Streaming para datasets grandes (no carga todo en memoria)
    IAsyncEnumerable<T> AsAsyncEnumerable(
        Expression<Func<T, bool>>? filter = null,
        bool tracking = false,
        params Expression<Func<T, object>>[] includes);

    // Specification Pattern para queries reutilizables
    Task<List<T>> GetBySpecification(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<T?> GetFirstBySpecification(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<int> GetCountBySpecification(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<PagedResult<T>> GetBySpecificationPaginated(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);
}
