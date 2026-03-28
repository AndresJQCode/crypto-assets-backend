using MediatR;

namespace Domain.SeedWork;

/// <summary>
/// Clase base abstracta para todas las entidades del dominio.
/// Implementa funcionalidades comunes como auditoría y eventos de dominio.
/// </summary>
/// <typeparam name="TId">Tipo del identificador de la entidad</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    private int? _requestedHashCode;
    private readonly HashSet<INotification> _domainEvents = [];

    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public virtual TId Id { get; protected set; } = default!;

    /// <summary>
    /// Fecha y hora de creación en UTC
    /// </summary>
    public DateTimeOffset CreatedOn { get; set; }

    /// <summary>
    /// Fecha y hora de última modificación en UTC
    /// </summary>
    public DateTimeOffset? LastModifiedOn { get; set; }

    /// <summary>
    /// Usuario que creó la entidad
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Usuario que modificó por última vez la entidad
    /// </summary>
    public Guid? LastModifiedBy { get; set; }

    /// <summary>
    /// Nombre del usuario que modificó por última vez la entidad
    /// </summary>
    public string? LastModifiedByName { get; set; }

    /// <summary>
    /// Colección de eventos de dominio de solo lectura
    /// </summary>
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents;

    /// <summary>
    /// Agrega un evento de dominio a la entidad
    /// </summary>
    /// <param name="eventItem">Evento de dominio a agregar</param>
    public void AddDomainEvent(INotification eventItem)
    {
        ArgumentNullException.ThrowIfNull(eventItem);
        _domainEvents.Add(eventItem);
    }

    /// <summary>
    /// Remueve un evento de dominio de la entidad
    /// </summary>
    /// <param name="eventItem">Evento de dominio a remover</param>
    public void RemoveDomainEvent(INotification eventItem)
    {
        ArgumentNullException.ThrowIfNull(eventItem);
        _domainEvents.Remove(eventItem);
    }

    /// <summary>
    /// Limpia todos los eventos de dominio de la entidad
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Determina si la entidad es transiente (sin ID asignado)
    /// </summary>
    /// <returns>True si la entidad es transiente, false en caso contrario</returns>
    public bool IsTransient() => EqualityComparer<TId>.Default.Equals(Id, default!);

    /// <summary>
    /// Determina si el objeto especificado es igual al objeto actual
    /// </summary>
    /// <param name="obj">Objeto a comparar</param>
    /// <returns>True si son iguales, false en caso contrario</returns>
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    /// <summary>
    /// Determina si la entidad especificada es igual a la entidad actual
    /// </summary>
    /// <param name="other">Entidad a comparar</param>
    /// <returns>True si son iguales, false en caso contrario</returns>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (IsTransient() || other.IsTransient())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Obtiene el código hash de la entidad
    /// </summary>
    /// <returns>Código hash de la entidad</returns>
    public override int GetHashCode()
    {
        if (IsTransient())
            return base.GetHashCode();

        if (!_requestedHashCode.HasValue)
        {
            _requestedHashCode = EqualityComparer<TId>.Default.GetHashCode(Id) ^ 31;
        }

        return _requestedHashCode.Value;
    }

    /// <summary>
    /// Operador de igualdad entre dos entidades
    /// </summary>
    /// <param name="left">Primera entidad</param>
    /// <param name="right">Segunda entidad</param>
    /// <returns>True si son iguales, false en caso contrario</returns>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Operador de desigualdad entre dos entidades
    /// </summary>
    /// <param name="left">Primera entidad</param>
    /// <param name="right">Segunda entidad</param>
    /// <returns>True si son diferentes, false en caso contrario</returns>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Clase base abstracta para entidades con identificador Guid
/// </summary>
public abstract class Entity : Entity<Guid>
{
    /// <summary>
    /// Constructor por defecto que inicializa el ID con un nuevo Guid
    /// </summary>
    protected Entity()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Constructor que permite especificar un ID personalizado
    /// </summary>
    /// <param name="id">ID personalizado para la entidad</param>
    protected Entity(Guid id)
    {
        Id = id;
    }
}
