using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public interface IQueryOptimizationService
{
    IQueryable<T> ApplyOptimizations<T>(IQueryable<T> query, bool enableSplitQuery = true) where T : class;
    Task<List<T>> ToListOptimizedAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class;
    Task<T?> FirstOrDefaultOptimizedAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class;
    Task<int> CountOptimizedAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class;
    Task<bool> AnyOptimizedAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class;
    IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int page, int pageSize) where T : class;
    IQueryable<T> ApplySearchFilter<T>(IQueryable<T> query, string searchTerm, params Expression<Func<T, string>>[] searchFields) where T : class;
}

public class QueryOptimizationService : IQueryOptimizationService
{
    private readonly ILogger<QueryOptimizationService> _logger;

    public QueryOptimizationService(ILogger<QueryOptimizationService> logger)
    {
        _logger = logger;
    }

    public IQueryable<T> ApplyOptimizations<T>(IQueryable<T> query, bool enableSplitQuery = true) where T : class
    {
        // Aplicar optimizaciones comunes
        var optimizedQuery = query.AsNoTracking(); // Para consultas de solo lectura

        if (enableSplitQuery)
        {
            optimizedQuery = optimizedQuery.AsSplitQuery(); // Para evitar cartesian explosion en joins
        }

        return optimizedQuery;
    }

    public async Task<List<T>> ToListOptimizedAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class
    {
        var optimizedQuery = ApplyOptimizations(query);

        _logger.LogDebug("Executing optimized ToList query for type: {Type}", typeof(T).Name);

        return await optimizedQuery.ToListAsync(cancellationToken);
    }

    public async Task<T?> FirstOrDefaultOptimizedAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class
    {
        var optimizedQuery = ApplyOptimizations(query);

        _logger.LogDebug("Executing optimized FirstOrDefault query for type: {Type}", typeof(T).Name);

        return await optimizedQuery.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountOptimizedAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class
    {
        var optimizedQuery = ApplyOptimizations(query, false); // No split query para count

        _logger.LogDebug("Executing optimized Count query for type: {Type}", typeof(T).Name);

        return await optimizedQuery.CountAsync(cancellationToken);
    }

    public async Task<bool> AnyOptimizedAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class
    {
        var optimizedQuery = ApplyOptimizations(query, false); // No split query para any

        _logger.LogDebug("Executing optimized Any query for type: {Type}", typeof(T).Name);

        return await optimizedQuery.AnyAsync(cancellationToken);
    }

    public IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int page, int pageSize) where T : class
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 10;

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public IQueryable<T> ApplySearchFilter<T>(IQueryable<T> query, string searchTerm, params Expression<Func<T, string>>[] searchFields) where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchFields == null || searchFields.Length == 0)
            return query;

        var searchTermUpper = searchTerm.ToUpperInvariant();

        // Crear expresión dinámica para búsqueda en múltiples campos
        var parameter = Expression.Parameter(typeof(T), "x");
        var searchExpressions = new List<Expression>();

        foreach (var field in searchFields)
        {
            // Convertir a mayúsculas y verificar si contiene el término de búsqueda
            var fieldAccess = Expression.Invoke(field, parameter);
            var toUpper = Expression.Call(fieldAccess, "ToUpperInvariant", null);
            var contains = Expression.Call(toUpper, "Contains", null, Expression.Constant(searchTermUpper));
            searchExpressions.Add(contains);
        }

        // Combinar todas las expresiones con OR
        Expression combinedExpression = searchExpressions[0];
        for (int i = 1; i < searchExpressions.Count; i++)
        {
            combinedExpression = Expression.OrElse(combinedExpression, searchExpressions[i]);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        return query.Where(lambda);
    }
}
