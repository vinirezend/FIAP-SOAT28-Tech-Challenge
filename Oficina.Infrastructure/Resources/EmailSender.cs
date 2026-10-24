
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;
using System.Net;
using System.Net.Mail;
using System.Text;

public class EmailSender : IEmailSender
{
    public async Task Enviar(EmailDTO email)
    {
        using var smtpClient = new SmtpClient(
            email.ServidorSMTP,
            email.PortaServidorSMTP)
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                email.Usuario,
                email.Senha),
            Timeout = 30000
        };

        using var message = new MailMessage
        {
            From = new MailAddress(
                email.EnderecoRemetente,
                email.NomeRemetente,
                Encoding.UTF8),

            Subject = email.Assunto,
            SubjectEncoding = Encoding.UTF8,
            Body = email.Mensagem,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };

        email.EnderecoDestinatario?.ForEach(
            endereco => message.To.Add(endereco));

        email.EnderecoCopia?.ForEach(
            endereco => message.CC.Add(endereco));

        try
        {
            await smtpClient.SendMailAsync(message);
        }
        catch (SmtpException ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}
