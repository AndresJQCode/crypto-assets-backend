using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Email;

/// <summary>
/// Adapter to bridge ASP.NET Core Identity's IEmailSender<TUser> with our IEmailService
/// This allows Identity framework to use our email service architecture
/// </summary>
public class IdentityEmailSenderAdapter<TUser>(IEmailService emailService) : IEmailSender<TUser>
    where TUser : class
{
    public async Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
    {
        var fullName = GetFullName(user);
        await emailService.SendConfirmationLinkAsync(email, fullName, confirmationLink);
    }

    public async Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
    {
        var fullName = GetFullName(user);
        await emailService.SendPasswordResetLinkAsync(email, fullName, resetLink);
    }

    public async Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
    {
        var fullName = GetFullName(user);
        await emailService.SendPasswordResetCodeAsync(email, fullName, resetCode);
    }

    private static string GetFullName(TUser user)
    {
        var firstName = user.GetType().GetProperty("FirstName")?.GetValue(user)?.ToString();
        var lastName = user.GetType().GetProperty("LastName")?.GetValue(user)?.ToString();
        var name = user.GetType().GetProperty("Name")?.GetValue(user)?.ToString();

        // Try Name first, then FirstName + LastName
        if (!string.IsNullOrEmpty(name))
            return name;

        if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
            return $"{firstName} {lastName}".Trim();

        return "Usuario";
    }
}
