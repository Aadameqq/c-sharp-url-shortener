using Core.Domain;
using Core.Ports;

namespace Infrastructure.Smtp;

public class ActivationCodeEmailSenderImpl(EmailSender emailSender) : ActivationCodeEmailSender
{
    public Task Send(Account account, string activationCode)
    {
        return emailSender.Send(
            account.Email,
            "Account activation",
            GetBody(account, activationCode)
        );
    }

    private string GetBody(Account account, string activationCode)
    {
        var html = File.ReadAllText(
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Smtp",
                "templates",
                "account-activation.html"
            )
        );

        html = html.Replace("{userName}", account.UserName);
        html = html.Replace("{code}", activationCode);

        return html;
    }
}
