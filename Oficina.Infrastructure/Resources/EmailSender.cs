using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;
using Oficina.Infrastructure.Configurations;

namespace Oficina.Infrastructure.Resources;

public class EmailSender(IOptions<EmailConfiguration> options) : IEmailSender
{
    private readonly EmailConfiguration emailConfiguration = options.Value;

    public async Task EnviarAsync(EmailDTO email, CancellationToken cancellationToken)
    {
        var emailMontado = MontarEmail(email);

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(emailConfiguration.Host, emailConfiguration.Port, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(emailConfiguration.User, emailConfiguration.Key, cancellationToken);
            var response = await client.SendAsync(emailMontado, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            Console.WriteLine($"Enviado! Resposta do servidor: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Falha no envio: {ex.Message}");
        }
    }

    private MimeMessage MontarEmail(EmailDTO email)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(emailConfiguration.AddressName, emailConfiguration.Address));
        message.To.Add(MailboxAddress.Parse(email.EnderecoDestinatario));
        message.Subject = email.Assunto;
        message.Body = new BodyBuilder
        {
            HtmlBody = email.CorpoHtml,
        }.ToMessageBody();

        return message;
    }
}
