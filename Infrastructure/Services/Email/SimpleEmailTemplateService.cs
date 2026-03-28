using Domain.Interfaces;

namespace Infrastructure.Services.Email;

public class SimpleEmailTemplateService : IEmailTemplateService
{
    private readonly string _baseTemplatePath;

    public SimpleEmailTemplateService()
    {
        _baseTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");
    }

    public async Task<string> RenderConfirmationEmailAsync(string fullName, string confirmationLink, string headerImage)
    {
        var template = await LoadTemplateAsync("ConfirmationEmail.html");
        return ReplaceVariables(template, new Dictionary<string, string>
                {
                        { "{{FULL_NAME}}", fullName },
                        { "{{CONFIRMATION_LINK}}", confirmationLink },
                        { "{{HEADER_IMAGE}}", headerImage },
                        { "{{COMPANY_NAME}}", "QCode" },
                        { "{{SUPPORT_EMAIL}}", "soporte@qcode.co" },
                        { "{{CURRENT_YEAR}}", DateTime.Now.Year.ToString(System.Globalization.CultureInfo.InvariantCulture) }
                });
    }

    public async Task<string> RenderPasswordResetEmailAsync(string fullName, string resetLink, string headerImage)
    {
        var template = await LoadTemplateAsync("PasswordResetEmail.html");
        return ReplaceVariables(template, new Dictionary<string, string>
                {
                        { "{{FULL_NAME}}", fullName },
                        { "{{RESET_LINK}}", resetLink },
                        { "{{HEADER_IMAGE}}", headerImage },
                        { "{{COMPANY_NAME}}", "QCode" },
                        { "{{SUPPORT_EMAIL}}", "soporte@qcode.co" },
                        { "{{CURRENT_YEAR}}", DateTime.Now.Year.ToString(System.Globalization.CultureInfo.InvariantCulture) }
                });
    }

    public async Task<string> RenderPasswordResetCodeEmailAsync(string fullName, string resetCode, string headerImage)
    {
        var template = await LoadTemplateAsync("PasswordResetCodeEmail.html");
        return ReplaceVariables(template, new Dictionary<string, string>
                {
                        { "{{FULL_NAME}}", fullName },
                        { "{{RESET_CODE}}", resetCode },
                        { "{{HEADER_IMAGE}}", headerImage },
                        { "{{COMPANY_NAME}}", "QCode" },
                        { "{{SUPPORT_EMAIL}}", "soporte@qcode.co" },
                        { "{{CURRENT_YEAR}}", DateTime.Now.Year.ToString(System.Globalization.CultureInfo.InvariantCulture) }
                });
    }

    private async Task<string> LoadTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine(_baseTemplatePath, templateName);

        if (!File.Exists(templatePath))
        {
            // Si no existe el archivo, usar plantilla por defecto
            return GetDefaultTemplate(templateName);
        }

        return await File.ReadAllTextAsync(templatePath);
    }

    private static string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        var result = template;
        foreach (var variable in variables)
        {
            result = result.Replace(variable.Key, variable.Value, StringComparison.Ordinal);
        }
        return result;
    }

    private static string GetDefaultTemplate(string templateName)
    {
        return templateName switch
        {
            "ConfirmationEmail.html" => GetDefaultConfirmationTemplate(),
            "PasswordResetEmail.html" => GetDefaultPasswordResetTemplate(),
            "PasswordResetCodeEmail.html" => GetDefaultPasswordResetCodeTemplate(),
            _ => GetDefaultLayout()
        };
    }

    private static string GetDefaultLayout()
    {
        return """
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>{{COMPANY_NAME}}</title>
            <style>
                body { margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4; }
                .email-container { max-width: 600px; margin: 0 auto; background-color: #ffffff; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); }
                .email-header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px 20px; text-align: center; }
                .email-header img { max-width: 150px; height: auto; }
                .email-content { padding: 0; }
                .email-footer { background-color: #2c3e50; color: #ecf0f1; padding: 30px 20px; text-align: center; }
                .email-footer a { color: #3498db; text-decoration: none; }
                .email-footer a:hover { text-decoration: underline; }
                .social-links { margin: 20px 0; }
                .social-links a { display: inline-block; margin: 0 10px; color: #ecf0f1; text-decoration: none; }
                @media only screen and (max-width: 600px) {
                    .email-container { width: 100% !important; }
                    .email-header, .email-footer { padding: 20px 15px !important; }
                }
            </style>
        </head>
        <body>
            <div class="email-container">
                <div class="email-header">
                    {{HEADER_IMAGE}}
                </div>
                <div class="email-content">
                    {{CONTENT}}
                </div>
                <div class="email-footer">
                    <p style="margin: 0 0 15px 0; font-size: 14px;">
                        © {{CURRENT_YEAR}} {{COMPANY_NAME}}. Todos los derechos reservados.
                    </p>
                    <div class="social-links">
                        <a href="https://qcode.co" target="_blank">Sitio Web</a>
                        <a href="mailto:{{SUPPORT_EMAIL}}">Soporte</a>
                        <a href="https://qcode.co/privacy" target="_blank">Privacidad</a>
                    </div>
                    <p style="margin: 15px 0 0 0; font-size: 12px; color: #bdc3c7;">
                        Este es un email automático, por favor no respondas a este mensaje.
                    </p>
                </div>
            </div>
        </body>
        </html>
        """;
    }

    private static string GetDefaultConfirmationTemplate()
    {
        var layout = GetDefaultLayout();
        var content = """
        <div style="text-align: center; padding: 40px 20px;">
            <h1 style="color: #2c3e50; font-family: Arial, sans-serif; margin-bottom: 30px;">
                ¡Bienvenido a {{COMPANY_NAME}}!
            </h1>
            
            <p style="color: #34495e; font-family: Arial, sans-serif; font-size: 16px; line-height: 1.6; margin-bottom: 30px;">
                Hola <strong>{{FULL_NAME}}</strong>,
            </p>
            
            <p style="color: #34495e; font-family: Arial, sans-serif; font-size: 16px; line-height: 1.6; margin-bottom: 30px;">
                Gracias por registrarte en nuestra plataforma. Para completar tu registro y activar tu cuenta, 
                por favor confirma tu dirección de email haciendo clic en el siguiente botón:
            </p>
            
            <div style="margin: 40px 0;">
                <a href="{{CONFIRMATION_LINK}}" 
                   style="background-color: #3498db; color: white; padding: 15px 30px; text-decoration: none; 
                          border-radius: 5px; font-family: Arial, sans-serif; font-size: 16px; font-weight: bold;
                          display: inline-block; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                    Confirmar Email
                </a>
            </div>
            
            <p style="color: #7f8c8d; font-family: Arial, sans-serif; font-size: 14px; line-height: 1.6; margin-top: 30px;">
                Si el botón no funciona, puedes copiar y pegar el siguiente enlace en tu navegador:
            </p>
            
            <p style="color: #3498db; font-family: Arial, sans-serif; font-size: 14px; word-break: break-all; 
                      background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;">
                {{CONFIRMATION_LINK}}
            </p>
            
            <p style="color: #7f8c8d; font-family: Arial, sans-serif; font-size: 14px; line-height: 1.6; margin-top: 30px;">
                Este enlace expirará en 24 horas por motivos de seguridad.
            </p>
            
            <p style="color: #7f8c8d; font-family: Arial, sans-serif; font-size: 14px; line-height: 1.6; margin-top: 30px;">
                Si no creaste una cuenta en {{COMPANY_NAME}}, puedes ignorar este email de forma segura.
            </p>
        </div>
        """;

        return layout.Replace("{{CONTENT}}", content, StringComparison.Ordinal);
    }

    private static string GetDefaultPasswordResetTemplate()
    {
        var layout = GetDefaultLayout();
        var content = """
        <div style="text-align: center; padding: 40px 20px;">
            <h1 style="color: #2c3e50; font-family: Arial, sans-serif; margin-bottom: 30px;">
                Restablecer Contraseña
            </h1>
            
            <p style="color: #34495e; font-family: Arial, sans-serif; font-size: 16px; line-height: 1.6; margin-bottom: 30px;">
                Hola <strong>{{FULL_NAME}}</strong>,
            </p>
            
            <p style="color: #34495e; font-family: Arial, sans-serif; font-size: 16px; line-height: 1.6; margin-bottom: 30px;">
                Recibimos una solicitud para restablecer la contraseña de tu cuenta en {{COMPANY_NAME}}. 
                Si fuiste tú quien hizo esta solicitud, haz clic en el siguiente botón para crear una nueva contraseña:
            </p>
            
            <div style="margin: 40px 0;">
                <a href="{{RESET_LINK}}" 
                   style="background-color: #e74c3c; color: white; padding: 15px 30px; text-decoration: none; 
                          border-radius: 5px; font-family: Arial, sans-serif; font-size: 16px; font-weight: bold;
                          display: inline-block; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                    Restablecer Contraseña
                </a>
            </div>
            
            <p style="color: #7f8c8d; font-family: Arial, sans-serif; font-size: 14px; line-height: 1.6; margin-top: 30px;">
                Si el botón no funciona, puedes copiar y pegar el siguiente enlace en tu navegador:
            </p>
            
            <p style="color: #e74c3c; font-family: Arial, sans-serif; font-size: 14px; word-break: break-all; 
                      background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;">
                {{RESET_LINK}}
            </p>
            
            <div style="background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 20px; margin: 30px 0;">
                <p style="color: #856404; font-family: Arial, sans-serif; font-size: 14px; line-height: 1.6; margin: 0;">
                    <strong>Importante:</strong> Este enlace expirará en 1 hora por motivos de seguridad. 
                    Si no restableces tu contraseña en este tiempo, deberás solicitar un nuevo enlace.
                </p>
            </div>
            
            <p style="color: #7f8c8d; font-family: Arial, sans-serif; font-size: 14px; line-height: 1.6; margin-top: 30px;">
                Si no solicitaste restablecer tu contraseña, puedes ignorar este email de forma segura. 
                Tu contraseña actual permanecerá sin cambios.
            </p>
            
            <p style="color: #7f8c8d; font-family: Arial, sans-serif; font-size: 14px; line-height: 1.6; margin-top: 20px;">
                Si tienes alguna pregunta, no dudes en contactarnos en <a href="mailto:{{SUPPORT_EMAIL}}" style="color: #3498db;">{{SUPPORT_EMAIL}}</a>
            </p>
        </div>
        """;

        return layout.Replace("{{CONTENT}}", content, StringComparison.Ordinal);
    }

    private static string GetDefaultPasswordResetCodeTemplate()
    {
        var layout = GetDefaultLayout();
        var content = """
        <div style="text-align: center; padding: 40px 20px;">
            <h1 style="color: #2c3e50; font-family: Arial, sans-serif; margin-bottom: 30px;">
                Código de Restablecimiento
            </h1>
            
            <p style="color: #34495e; font-family: Arial, sans-serif; font-size: 16px; line-height: 1.6; margin-bottom: 30px;">
                Hola <strong>{{FULL_NAME}}</strong>,
            </p>
            
            <p style="color: #34495e; font-family: Arial, sans-serif; font-size: 16px; line-height: 1.6; margin-bottom: 30px;">
                Recibimos una solicitud para restablecer la contraseña de tu cuenta en {{COMPANY_NAME}}. 
                Utiliza el siguiente código para completar el proceso:
            </p>
            
            <div style="margin: 40px 0;">
                <div style="background-color: #f8f9fa; border: 2px solid #e9ecef; border-radius: 10px; 
                            padding: 30px; display: inline-block; min-width: 200px;">
                    <p style="color: #495057; font-family: Arial, sans-serif; font-size: 14px; margin: 0 0 10px 0;">
                        Tu código de restablecimiento es:
                    </p>
                    <div style="background-color: #e74c3c; color: white; padding: 20px; border-radius: 8px; 
                                font-family: 'Courier New', monospace; font-size: 24px; font-weight: bold; 
                                letter-spacing: 3px; margin: 0;">
                        {{RESET_CODE}}
                    </div>
                </div>
            </div>
            
            <div style="background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 20px; margin: 30px 0;">
                <p style="color: #856404; font-family: Arial, sans-serif; font-size: 14px; line-height: 1.6; margin: 0;">
                    <strong>Importante:</strong> Este código expirará en 15 minutos por motivos de seguridad. 
                    Si no lo utilizas en este tiempo, deberás solicitar un nuevo código.
                </p>
            </div>
            
            <p style="color: #34495e; font-family: Arial, sans-serif; font-size: 16px; line-height: 1.6; margin-top: 30px;">
                Ingresa este código en la aplicación para restablecer tu contraseña.
            </p>
            
            <p style="color: #7f8c8d; font-family: Arial, sans-serif; font-size: 14px; line-height: 1.6; margin-top: 30px;">
                Si no solicitaste restablecer tu contraseña, puedes ignorar este email de forma segura. 
                Tu contraseña actual permanecerá sin cambios.
            </p>
            
            <p style="color: #7f8c8d; font-family: Arial, sans-serif; font-size: 14px; line-height: 1.6; margin-top: 20px;">
                Si tienes alguna pregunta, no dudes en contactarnos en <a href="mailto:{{SUPPORT_EMAIL}}" style="color: #3498db;">{{SUPPORT_EMAIL}}</a>
            </p>
        </div>
        """;

        return layout.Replace("{{CONTENT}}", content, StringComparison.Ordinal);
    }
}
