using Core.Domain;
using Core.Ports;

namespace Infrastructure.Smtp;

public class PasswordResetEmailSenderImpl(EmailSender emailSender) : PasswordResetEmailSender
{
    public Task Send(Account account, string resetCode)
    {
        return emailSender.Send(account.Email, "Password Reset", GetBody(account, resetCode));
    }

    private string GetBody(Account account, string resetCode)
    {
        var html = File.ReadAllText(
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Smtp",
                "templates",
                "password-reset.html"
            )
        );

        html = html.Replace("{userName}", account.UserName);
        html = html.Replace("{code}", resetCode);

        return html;
    }
}
