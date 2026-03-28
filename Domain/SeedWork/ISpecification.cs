using System.Linq.Expressions;

namespace Domain.SeedWork;

/// <summary>
/// Specification Pattern para encapsular queries complejas reutilizables
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Criterio de filtro principal
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Propiedades de navegación a incluir (eager loading)
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Ordenamiento ascendente
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Ordenamiento descendente
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Si debe habilitar tracking de cambios (default: false)
    /// </summary>
    bool AsTracking { get; }

    /// <summary>
    /// Número de página para paginación (null = sin paginación)
    /// </summary>
    int? Page { get; }

    /// <summary>
    /// Tamaño de página para paginación
    /// </summary>
    int PageSize { get; }
}
