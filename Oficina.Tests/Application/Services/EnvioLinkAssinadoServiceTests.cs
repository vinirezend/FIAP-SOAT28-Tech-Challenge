using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Implementations.Services;
using Oficina.Application.Interfaces.Resources;
using Oficina.Application.Templates;

namespace Oficina.Tests.Application.Services;

public class EnvioLinkAssinadoServiceTests
{
    private const string Caminho = "/api/cliente/1/compartilhado";

    private readonly Mock<ILinkAssinadoGenerator> _linkAssinadoGeneratorMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly EnvioLinkAssinadoService _service;
    private readonly LinkAssinadoDTO _link = new()
    {
        Url = "https://api.oficina.test/api/cliente/1/compartilhado?expira=1&assinatura=abc",
        ExpiraEm = new DateTimeOffset(2026, 1, 1, 15, 0, 0, TimeSpan.Zero)
    };

    private EmailDTO? _emailEnviado;

    public EnvioLinkAssinadoServiceTests()
    {
        _linkAssinadoGeneratorMock
            .Setup(g => g.Gerar(Caminho, It.IsAny<int?>()))
            .Returns(_link);

        _emailSenderMock
            .Setup(s => s.EnviarAsync(It.IsAny<EmailDTO>(), It.IsAny<CancellationToken>()))
            .Callback<EmailDTO, CancellationToken>((email, _) => _emailEnviado = email)
            .Returns(Task.CompletedTask);

        _service = new EnvioLinkAssinadoService(_linkAssinadoGeneratorMock.Object, _emailSenderMock.Object);
    }

    private static EnvioLinkAssinadoDTO CriarEnvio(string assunto, string templateHtml, Dictionary<string, string>? parametros = null) => new()
    {
        Caminho = Caminho,
        EmailDestinatario = "cliente@teste.com",
        Assunto = assunto,
        TemplateHtml = templateHtml,
        Parametros = parametros ?? [],
        MinutosValidade = 30
    };

    [Fact]
    public async Task EnviarAsync_TemplateDadosCliente_DeveGerarLinkEEnviarEmailParaODestinatario()
    {
        // Arrange
        var envio = CriarEnvio(TemplatesEmail.DadosClienteAssunto, TemplatesEmail.DadosClienteHtml, new() { ["nome"] = "Ana <Silva>" });

        // Act
        var resultado = await _service.EnviarAsync(envio, CancellationToken.None);

        // Assert
        Assert.Same(_link, resultado);
        _linkAssinadoGeneratorMock.Verify(g => g.Gerar(Caminho, 30), Times.Once);
        Assert.NotNull(_emailEnviado);
        Assert.Equal("cliente@teste.com", _emailEnviado!.EnderecoDestinatario);
        Assert.Equal(TemplatesEmail.DadosClienteAssunto, _emailEnviado.Assunto);
        Assert.Contains("href=\"https://api.oficina.test/api/cliente/1/compartilhado?expira=1&amp;assinatura=abc\"", _emailEnviado.CorpoHtml);
        Assert.Contains("Ana &lt;Silva&gt;", _emailEnviado.CorpoHtml);
        Assert.Contains("01/01/2026 às 12:00", _emailEnviado.CorpoHtml);
        Assert.DoesNotContain("{{", _emailEnviado.CorpoHtml);
    }

    [Fact]
    public async Task EnviarAsync_TemplatePersonalizado_DevePreencherTodosOsPlaceholders()
    {
        // Arrange
        const string templateHtml = "<p>OS {{ numeroOs }} de {{NOME}}: <a href=\"{{link}}\">ver orçamento</a> até {{expiraEm}}</p>";
        var envio = CriarEnvio("Orçamento disponível", templateHtml, new() { ["nome"] = "Ana", ["numeroOs"] = "123" });

        // Act
        await _service.EnviarAsync(envio, CancellationToken.None);

        // Assert
        Assert.Equal("Orçamento disponível", _emailEnviado!.Assunto);
        Assert.Equal(
            "<p>OS 123 de Ana: <a href=\"https://api.oficina.test/api/cliente/1/compartilhado?expira=1&amp;assinatura=abc\">ver orçamento</a> até 01/01/2026 às 12:00</p>",
            _emailEnviado.CorpoHtml);
    }

    [Fact]
    public async Task EnviarAsync_PlaceholderSemValor_DeveLancarExcecaoSemEnviarEmail()
    {
        // Arrange
        var envio = CriarEnvio(TemplatesEmail.DadosClienteAssunto, TemplatesEmail.DadosClienteHtml);

        // Act
        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.EnviarAsync(envio, CancellationToken.None));

        // Assert
        Assert.Contains("{{nome}}", excecao.Message);
        _emailSenderMock.Verify(s => s.EnviarAsync(It.IsAny<EmailDTO>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
