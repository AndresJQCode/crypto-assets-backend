namespace Domain.Interfaces;

/// <summary>
/// Servicio para operaciones de caché
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtiene un valor del caché
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="key">Clave del caché</param>
    /// <returns>Valor del caché o null si no existe</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Establece un valor en el caché
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="key">Clave del caché</param>
    /// <param name="value">Valor a almacenar</param>
    /// <param name="expiration">Tiempo de expiración (opcional)</param>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Elimina un valor del caché
    /// </summary>
    /// <param name="key">Clave del caché</param>
    void Remove(string key);

    /// <summary>
    /// Elimina valores del caché que coinciden con un patrón
    /// </summary>
    /// <param name="pattern">Patrón de búsqueda</param>
    void RemoveByPattern(string pattern);

    /// <summary>
    /// Verifica si existe una clave en el caché
    /// </summary>
    /// <param name="key">Clave del caché</param>
    /// <returns>True si existe, false en caso contrario</returns>
    bool Exists(string key);

    /// <summary>
    /// Obtiene un valor del caché o lo genera si no existe
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="key">Clave del caché</param>
    /// <param name="factory">Función para generar el valor si no existe</param>
    /// <param name="expiration">Tiempo de expiración (opcional)</param>
    /// <returns>Valor del caché o generado</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}
