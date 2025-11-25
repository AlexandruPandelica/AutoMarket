using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // In productie, aici trimiți email-ul real
        Console.WriteLine($"Email trimis catre {email} cu subiectul '{subject}'");
        return Task.CompletedTask;
    }
}