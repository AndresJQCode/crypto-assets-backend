using Domain.AggregatesModel.UserAggregate;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Services.Email;
using Infrastructure.Services.Email.Providers;
using Infrastructure.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Api.Extensions;

/// <summary>
/// Email services registration with Strategy pattern
/// Allows switching between different email providers (Infobip, SendGrid, SMTP, etc.)
/// Uses IValidateOptions pattern for configuration validation
/// </summary>
internal static class EmailServicesExtensions
{
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register configuration validators (IValidateOptions pattern)
        services.AddSingleton<IValidateOptions<AppSettings>, EmailServiceOptionsValidator>();
        services.AddSingleton<IValidateOptions<AppSettings>, InfobipOptionsValidator>();
        // Future: services.AddSingleton<IValidateOptions<AppSettings>, SendGridOptionsValidator>();

        // Validate configuration on startup (fail-fast)
        services.AddOptions<AppSettings>()
            .Bind(configuration)
            .ValidateOnStart();

        // Register email template service
        services.AddTransient<IEmailTemplateService, SimpleEmailTemplateService>();

        // Get validated configuration
        var emailSettings = configuration.GetSection("EmailService").Get<AppSettings.EmailSettings>();
        if (emailSettings == null)
        {
            throw new InvalidOperationException(
                "EmailService configuration section is missing. " +
                "This should have been caught by EmailServiceOptionsValidator.");
        }

        // Register email provider based on configuration (Strategy pattern)
        var provider = emailSettings.Provider?.ToUpperInvariant() ?? "INFOBIP";

        switch (provider)
        {
            case "INFOBIP":
                services.AddSingleton<IEmailProvider, InfobipEmailProvider>();
                break;

            // Future providers can be added here:
            // case "SENDGRID":
            //     services.AddSingleton<IEmailProvider, SendGridEmailProvider>();
            //     break;
            //
            // case "SMTP":
            //     services.AddSingleton<IEmailProvider, SmtpEmailProvider>();
            //     break;

            default:
                throw new InvalidOperationException(
                    $"Unknown email provider '{emailSettings.Provider}'. " +
                    $"Supported providers: Infobip");
        }

        // Register main email service
        services.AddScoped<IEmailService, EmailService>();

        // Register Identity email sender adapter (bridges Identity with our email service)
        services.AddTransient<IEmailSender<User>, IdentityEmailSenderAdapter<User>>();

        return services;
    }
}

