namespace Api.Constants;

/// <summary>
/// Constants for error messages used throughout the application.
/// </summary>
internal static class ErrorMessageConstants
{
    /// <summary>
    /// Authentication and authorization error messages
    /// </summary>
    internal static class Auth
    {
        public const string Unauthorized = "No autorizado";
        public const string NotAuthenticated = "El usuario no está autenticado";
        public const string InvalidCredentials = "Credenciales inválidas";
        public const string InvalidToken = "Token inválido o expirado";
    }

    /// <summary>
    /// Permission-related error messages
    /// </summary>
    internal static class Permission
    {
        public const string Denied = "Permiso denegado";
        public const string UserLacksPermission = "El usuario {0} no tiene permiso {1}.{2}";
        public const string InvalidClaim = "Claim de usuario inválido";
    }

    /// <summary>
    /// General error messages
    /// </summary>
    internal static class General
    {
        public const string InternalServerError = "Error interno del servidor";
        public const string UnexpectedError = "Ocurrió un error inesperado";
        public const string NotFound = "Recurso no encontrado";
        public const string BadRequest = "Solicitud inválida";
    }
}
