using Microsoft.Extensions.Options;
using MimeKit;
using Oficina.Application.DTOs;
using Oficina.Infrastructure.Configurations;
using Oficina.Infrastructure.Resources;

namespace Oficina.Tests.Infraestructure.Resources;

public class EmailSenderTests
{
    private readonly EmailSender _emailSender;

    public EmailSenderTests()
    {
        // Porta 1 no localhost: a conexão é recusada sem depender de um servidor SMTP real
        var configuration = new EmailConfiguration
        {
            Host = "localhost",
            Port = 1,
            User = "usuario-teste",
            Key = "chave-teste",
            Address = "remetente@teste.com",
            AddressName = "Oficina Teste"
        };

        _emailSender = new EmailSender(Options.Create(configuration));
    }

    private static EmailDTO CriarEmail(string destinatario = "destinatario@teste.com")
    {
        return new EmailDTO
        {
            Assunto = "Assunto de teste",
            CorpoHtml = "<p>Corpo de teste</p>",
            EnderecoDestinatario = destinatario
        };
    }

    [Fact]
    public async Task EnviarAsync_FalhaNaConexaoSmtp_NaoDeveLancarExcecao()
    {
        // Act
        var excecao = await Record.ExceptionAsync(
            () => _emailSender.EnviarAsync(CriarEmail(), CancellationToken.None));

        // Assert
        Assert.Null(excecao);
    }

    [Theory]
    [InlineData("")]
    [InlineData("destinatario@")]
    public async Task EnviarAsync_DestinatarioInvalido_DeveLancarParseException(string destinatario)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ParseException>(
            () => _emailSender.EnviarAsync(CriarEmail(destinatario), CancellationToken.None));
    }
}
