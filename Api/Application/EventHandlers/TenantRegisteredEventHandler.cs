using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Events;
using Domain.Interfaces;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Application.EventHandlers;

/// <summary>
/// Handles TenantRegisteredEvent by sending notification emails to all SuperAdmin users
/// </summary>
public class TenantRegisteredEventHandler(
    UserManager<User> userManager,
    IEmailService emailService,
    IOptionsMonitor<AppSettings> appSettings,
    ILogger<TenantRegisteredEventHandler> logger) : INotificationHandler<TenantRegisteredEvent>
{
    private string FromEmail => appSettings.CurrentValue.EmailService.FromEmail;
    public async Task Handle(TenantRegisteredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get all SuperAdmin users
            var superAdmins = await GetSuperAdminUsersAsync(cancellationToken);

            if (superAdmins.Count == 0)
            {
                logger.LogWarning("No SuperAdmin users found to notify about tenant registration");
                return;
            }

            logger.LogInformation("Found {Count} SuperAdmin users to notify", superAdmins.Count);

            // Send email to each SuperAdmin
            foreach (var superAdmin in superAdmins)
            {
                await SendRegistrationEmailAsync(superAdmin, notification, cancellationToken);
            }

            logger.LogInformation(
                "Successfully sent {Count} notification emails for tenant registration: {TenantName}",
                superAdmins.Count,
                notification.TenantName);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - email notification failure shouldn't break registration
            logger.LogError(
                ex,
                "Error sending notification emails for tenant registration: {TenantId}",
                notification.TenantId);
        }
    }

    private async Task<List<User>> GetSuperAdminUsersAsync(CancellationToken cancellationToken)
    {
        // Get all users with SuperAdmin role
        var superAdminUsers = await userManager.GetUsersInRoleAsync(RolesEnum.SuperAdmin.ToString());

        // Filter to only active users with confirmed emails
        return superAdminUsers
            .Where(u => u.IsActive && u.EmailConfirmed)
            .ToList();
    }

    private async Task SendRegistrationEmailAsync(
        User superAdmin,
        TenantRegisteredEvent notification,
        CancellationToken cancellationToken)
    {
        try
        {
            var subject = $"🎉 Nuevo Tenant Registrado en Lulo CRM";

            var message = $@"
<h2>Nuevo Tenant Registrado en el Sistema</h2>

<p>Se ha registrado un nuevo tenant en la plataforma:</p>

<table style='border-collapse: collapse; margin: 20px 0;'>
    <tr>
        <td style='padding: 8px; font-weight: bold;'>Tenant:</td>
        <td style='padding: 8px;'>{notification.TenantName}</td>
    </tr>
    <tr>
        <td style='padding: 8px; font-weight: bold;'>Slug:</td>
        <td style='padding: 8px;'>{notification.TenantSlug}</td>
    </tr>
    <tr>
        <td style='padding: 8px; font-weight: bold;'>Administrador:</td>
        <td style='padding: 8px;'>{notification.AdminName}</td>
    </tr>
    <tr>
        <td style='padding: 8px; font-weight: bold;'>Email:</td>
        <td style='padding: 8px;'>{notification.AdminEmail}</td>
    </tr>
    <tr>
        <td style='padding: 8px; font-weight: bold;'>Fecha de Registro:</td>
        <td style='padding: 8px;'>{notification.RegisteredAt:dd/MM/yyyy HH:mm} UTC</td>
    </tr>
</table>

<p>Este es un mensaje automático generado por el sistema.</p>
";

            await emailService.SendCustomEmailAsync(
                from: FromEmail,
                to: new[] { superAdmin.Email! },
                subject: subject,
                htmlBody: message,
                cancellationToken: cancellationToken);

            logger.LogDebug(
                "Notification email sent to SuperAdmin: {Email}",
                superAdmin.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send email to SuperAdmin: {Email}",
                superAdmin.Email);
            // Don't rethrow - continue sending to other admins
        }
    }
}
