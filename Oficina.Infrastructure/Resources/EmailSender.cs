
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;

namespace Oficina.Infrastructure.Resources;

public class EmailSender : IEmailSender
{
    private const string host = "smtp-relay.brevo.com";
    private const int port = 587;
    private readonly string user = Environment.GetEnvironmentVariable("BREVO_SMTP_USER") ?? string.Empty;
    private readonly string key = Environment.GetEnvironmentVariable("BREVO_SMTP_KEY") ?? string.Empty;

    public async Task Enviar(EmailDTO email)
    {
        var emailMontado = MontarEmail(email);

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, key);
            var response = await client.SendAsync(emailMontado);
            await client.DisconnectAsync(true);
            Console.WriteLine($"Enviado! Resposta do servidor: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Falha no envio: {ex.Message}");
        }
    }

    private static MimeMessage MontarEmail(EmailDTO email)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("POC Pós", "betojunior1250@gmail.com"));
        message.To.Add(MailboxAddress.Parse(email.EnderecoDestinatario));
        message.Subject = email.Assunto;
        message.Body = new BodyBuilder
        {
            HtmlBody = email.CorpoHtml,
        }.ToMessageBody();

        return message;
    }
}
