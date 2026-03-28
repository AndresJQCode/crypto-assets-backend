namespace Domain.Interfaces;

/// <summary>
/// Servicio para renderizar plantillas de email
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Renderiza la plantilla de email de confirmación
    /// </summary>
    /// <param name="fullName">Nombre completo del usuario</param>
    /// <param name="confirmationLink">Enlace de confirmación</param>
    /// <param name="headerImage">URL de la imagen del encabezado</param>
    /// <returns>HTML renderizado del email</returns>
    Task<string> RenderConfirmationEmailAsync(string fullName, string confirmationLink, string headerImage);

    /// <summary>
    /// Renderiza la plantilla de email de restablecimiento de contraseña
    /// </summary>
    /// <param name="fullName">Nombre completo del usuario</param>
    /// <param name="resetLink">Enlace de restablecimiento</param>
    /// <param name="headerImage">URL de la imagen del encabezado</param>
    /// <returns>HTML renderizado del email</returns>
    Task<string> RenderPasswordResetEmailAsync(string fullName, string resetLink, string headerImage);

    /// <summary>
    /// Renderiza la plantilla de email de código de restablecimiento de contraseña
    /// </summary>
    /// <param name="fullName">Nombre completo del usuario</param>
    /// <param name="resetCode">Código de restablecimiento</param>
    /// <param name="headerImage">URL de la imagen del encabezado</param>
    /// <returns>HTML renderizado del email</returns>
    Task<string> RenderPasswordResetCodeEmailAsync(string fullName, string resetCode, string headerImage);
}
