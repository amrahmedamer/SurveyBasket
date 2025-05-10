using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using SurveyBasket.Api.Settings;

namespace SurveyBasket.Api.Services;

public class EmailService(IOptions<MailSetting> mailsetting) : IEmailSender
{
    private readonly MailSetting _mailsetting = mailsetting.Value;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage
        {
            Sender = MailboxAddress.Parse(_mailsetting.Mail),
            Subject = subject
        };

        message.To.Add(MailboxAddress.Parse(email));

        var builder = new BodyBuilder()
        {
            HtmlBody = htmlMessage
        };

        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        smtp.Connect(_mailsetting.Host, _mailsetting.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailsetting.Mail,_mailsetting.Password);
        await smtp.SendAsync(message);
        smtp.Disconnect(true);
    }
}
