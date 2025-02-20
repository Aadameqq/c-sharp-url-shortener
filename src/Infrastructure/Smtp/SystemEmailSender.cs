using System.Net.Mail;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Smtp;

public class SystemEmailSender(IOptions<SmtpOptions> smtpOptions) : EmailSender
{
    public Task Send(string to, string subject, string body)
    {
        var client = new SmtpClient(smtpOptions.Value.Host, smtpOptions.Value.Port);
        client.EnableSsl = false;
        client.UseDefaultCredentials = false;

        var message = new MailMessage();
        message.From = new MailAddress(smtpOptions.Value.Email);
        message.To.Add(new MailAddress(to));
        message.Subject = subject;
        message.IsBodyHtml = true;
        message.Body = body;

        client.SendAsync(message, null);

        return Task.CompletedTask;
    }
}
