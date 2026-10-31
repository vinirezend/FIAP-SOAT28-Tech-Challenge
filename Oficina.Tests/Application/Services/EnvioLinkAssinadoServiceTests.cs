using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Implementations.Services;
using Oficina.Application.Interfaces.Resources;
using Oficina.Application.Templates;

namespace Oficina.Tests.Application.Services;

public class EnvioLinkAssinadoServiceTests
{
    private const string Caminho = "/api/acompanhamento-ordem-servico/1/cliente";

    private readonly Mock<ILinkAssinadoGenerator> _linkAssinadoGeneratorMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly EnvioLinkAssinadoService _service;
    private readonly LinkAssinadoDTO _link = new()
    {
        Url = "https://api.oficina.test/api/acompanhamento-ordem-servico/1/cliente?expira=1&assinatura=abc",
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

    private static Dictionary<string, string> ParametrosAcompanhamentoOrdemServico() => new()
    {
        ["nome"] = "Ana <Silva>",
        ["idOs"] = "1",
        ["veiculo"] = "Volkswagen Gol (FFF2026)",
        ["status"] = "Em diagnóstico"
    };

    private static EnvioLinkAssinadoDTO CriarEnvio(Dictionary<string, string>? parametros = null) => new()
    {
        Caminho = Caminho,
        EmailDestinatario = "cliente@teste.com",
        Assunto = TemplatesEmail.AcompanhamentoOrdemServicoAssunto,
        Template = NomesTemplate.AcompanhamentoOrdemServico.Email,
        Parametros = parametros ?? ParametrosAcompanhamentoOrdemServico(),
        MinutosValidade = 30
    };

    [Fact]
    public async Task EnviarAsync_TemplateAcompanhamentoOrdemServico_DeveGerarLinkEEnviarEmailParaODestinatario()
    {
        // Arrange
        var envio = CriarEnvio();

        // Act
        var resultado = await _service.EnviarAsync(envio, CancellationToken.None);

        // Assert
        Assert.Equal(_link.Url, resultado.Url);
        Assert.Equal(_link.ExpiraEm, resultado.ExpiraEm);
        _linkAssinadoGeneratorMock.Verify(g => g.Gerar(Caminho, 30), Times.Once);
        Assert.NotNull(_emailEnviado);
        Assert.Equal("cliente@teste.com", _emailEnviado!.EnderecoDestinatario);
        Assert.Equal(TemplatesEmail.AcompanhamentoOrdemServicoAssunto, _emailEnviado.Assunto);
        Assert.Contains("href=\"https://api.oficina.test/api/acompanhamento-ordem-servico/1/cliente?expira=1&amp;assinatura=abc\"", _emailEnviado.CorpoHtml);
        Assert.Contains("Ana &lt;Silva&gt;", _emailEnviado.CorpoHtml);
        Assert.Contains("01/01/2026 às 12:00", _emailEnviado.CorpoHtml);
        Assert.DoesNotContain("{{", _emailEnviado.CorpoHtml);
    }

    [Fact]
    public async Task EnviarAsync_PlaceholderSemValor_DeveLancarExcecaoSemEnviarEmail()
    {
        // Arrange
        var parametros = ParametrosAcompanhamentoOrdemServico();
        parametros.Remove("nome");
        var envio = CriarEnvio(parametros);

        // Act
        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.EnviarAsync(envio, CancellationToken.None));

        // Assert
        Assert.Contains("{{nome}}", excecao.Message);
        _emailSenderMock.Verify(s => s.EnviarAsync(It.IsAny<EmailDTO>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnviarAsync_LinksAssinadosEParametrosHtml_DeveGerarCadaLinkEInserirHtmlSemCodificar()
    {
        // Arrange
        _linkAssinadoGeneratorMock
            .Setup(g => g.Gerar("/api/orcamento/1/aprovar", 30))
            .Returns(new LinkAssinadoDTO { Url = "https://api.test/aprovar?a=1&b=2", ExpiraEm = _link.ExpiraEm });
        _linkAssinadoGeneratorMock
            .Setup(g => g.Gerar("/api/orcamento/1/recusar", 30))
            .Returns(new LinkAssinadoDTO { Url = "https://api.test/recusar", ExpiraEm = _link.ExpiraEm });

        var envio = new EnvioLinkAssinadoDTO
        {
            EmailDestinatario = "cliente@teste.com",
            Assunto = TemplatesEmail.OrcamentoAssunto,
            Template = NomesTemplate.Orcamento.Email,
            Parametros = new Dictionary<string, string>
            {
                ["nome"] = "Ana",
                ["idOs"] = "1",
                ["veiculo"] = "Volkswagen Gol (FFF2026)",
                ["valorTotal"] = "R$ 10,00"
            },
            ParametrosHtml = new Dictionary<string, string> { ["itens"] = "<tr><td>Óleo</td></tr>" },
            LinksAssinados = new Dictionary<string, string> { ["aprovar"] = "/api/orcamento/1/aprovar", ["recusar"] = "/api/orcamento/1/recusar" },
            MinutosValidade = 30
        };

        // Act
        var resultado = await _service.EnviarAsync(envio, CancellationToken.None);

        // Assert
        Assert.Empty(resultado.Url);
        Assert.Equal("https://api.test/aprovar?a=1&b=2", resultado.Links["aprovar"]);
        Assert.Equal("https://api.test/recusar", resultado.Links["recusar"]);
        Assert.Contains("<tbody><tr><td>Óleo</td></tr></tbody>", _emailEnviado!.CorpoHtml);
        Assert.Contains("href=\"https://api.test/aprovar?a=1&amp;b=2\"", _emailEnviado.CorpoHtml);
        Assert.Contains("href=\"https://api.test/recusar\"", _emailEnviado.CorpoHtml);
        Assert.Contains("01/01/2026 às 12:00", _emailEnviado.CorpoHtml);
    }

    [Fact]
    public async Task EnviarAsync_SemNenhumCaminho_DeveLancarExcecaoSemEnviarEmail()
    {
        // Arrange
        var envio = CriarEnvio();
        envio.Caminho = null;

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.EnviarAsync(envio, CancellationToken.None));
        _emailSenderMock.Verify(s => s.EnviarAsync(It.IsAny<EmailDTO>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
