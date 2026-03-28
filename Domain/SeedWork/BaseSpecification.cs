using System.Linq.Expressions;

namespace Domain.SeedWork;

/// <summary>
/// Implementación base del Specification Pattern
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification()
    {
    }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    private readonly List<Expression<Func<T, object>>> _includes = [];

    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public bool AsTracking { get; private set; }
    public int? Page { get; private set; }
    public int PageSize { get; private set; } = 10;

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        _includes.Add(includeExpression);
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    protected void ApplyTracking()
    {
        AsTracking = true;
    }

    protected void ApplyPaging(int page, int pageSize = 10)
    {
        if (page < 1)
            throw new ArgumentException("Page debe ser mayor a 0", nameof(page));

        if (pageSize < 1)
            throw new ArgumentException("Page size debe ser mayor a 0", nameof(pageSize));

        Page = page;
        PageSize = pageSize;
    }
}
