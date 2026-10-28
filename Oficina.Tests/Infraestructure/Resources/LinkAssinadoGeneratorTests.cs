using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Oficina.Application.Interfaces.Resources;
using Oficina.Infrastructure.Configurations;
using Oficina.Infrastructure.Resources;

namespace Oficina.Tests.Infraestructure.Resources;

public class LinkAssinadoGeneratorTests
{
    private const string Caminho = "/api/cliente/1/compartilhado";

    private readonly RelogioFixo _relogio = new(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly LinkAssinadoConfiguration _configuration = new()
    {
        Secret = "chave-de-teste-com-tamanho-suficiente-123456",
        UrlBase = "https://api.oficina.test/",
        MinutosValidadePadrao = 60,
        MinutosValidadeMaxima = 120
    };

    private LinkAssinadoGenerator CriarGerador() =>
        new(Options.Create(_configuration), _httpContextAccessorMock.Object, _relogio);

    private static (string Caminho, string? Expira, string? Assinatura) ExtrairPartes(string url)
    {
        var uri = new Uri(url);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        return (uri.AbsolutePath, query[ParametrosLinkAssinado.Expira], query[ParametrosLinkAssinado.Assinatura]);
    }

    [Fact]
    public void Gerar_DeveMontarUrlComUrlBaseExpiracaoEAssinatura()
    {
        var link = CriarGerador().Gerar(Caminho);

        Assert.StartsWith("https://api.oficina.test/api/cliente/1/compartilhado?expira=", link.Url);
        Assert.Contains("&assinatura=", link.Url);
        Assert.Equal(_relogio.Agora.AddMinutes(60), link.ExpiraEm);
    }

    [Theory]
    [InlineData(500, 120)]
    [InlineData(0, 1)]
    [InlineData(30, 30)]
    public void Gerar_DeveLimitarValidadeEntreUmMinutoEOMaximo(int minutosSolicitados, int minutosEsperados)
    {
        var link = CriarGerador().Gerar(Caminho, minutosSolicitados);

        Assert.Equal(_relogio.Agora.AddMinutes(minutosEsperados), link.ExpiraEm);
    }

    [Fact]
    public void Gerar_SemUrlBase_DeveUsarHostDaRequisicao()
    {
        _configuration.UrlBase = string.Empty;
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost", 5078);
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var link = CriarGerador().Gerar(Caminho);

        Assert.StartsWith("http://localhost:5078/api/cliente/1/compartilhado?", link.Url);
    }

    [Fact]
    public void Gerar_SemUrlBaseESemRequisicao_DeveLancarInvalidOperationException()
    {
        _configuration.UrlBase = string.Empty;

        Assert.Throws<InvalidOperationException>(() => CriarGerador().Gerar(Caminho));
    }

    [Fact]
    public void Validar_LinkGerado_DeveSerValido()
    {
        var gerador = CriarGerador();
        var (caminho, expira, assinatura) = ExtrairPartes(gerador.Gerar(Caminho).Url);

        Assert.Equal(StatusLinkAssinado.Valido, gerador.Validar(caminho, expira, assinatura));
    }

    [Fact]
    public void Validar_CaminhoComMaiusculasOuBarraFinal_DeveSerValido()
    {
        var gerador = CriarGerador();
        var (_, expira, assinatura) = ExtrairPartes(gerador.Gerar(Caminho).Url);

        Assert.Equal(StatusLinkAssinado.Valido, gerador.Validar("/api/Cliente/1/compartilhado/", expira, assinatura));
    }

    [Fact]
    public void Validar_AposExpiracao_DeveRetornarExpirado()
    {
        var gerador = CriarGerador();
        var (caminho, expira, assinatura) = ExtrairPartes(gerador.Gerar(Caminho, 30).Url);

        _relogio.Agora = _relogio.Agora.AddMinutes(31);

        Assert.Equal(StatusLinkAssinado.Expirado, gerador.Validar(caminho, expira, assinatura));
    }

    [Fact]
    public void Validar_CaminhoAlterado_DeveRetornarInvalido()
    {
        var gerador = CriarGerador();
        var (_, expira, assinatura) = ExtrairPartes(gerador.Gerar(Caminho).Url);

        Assert.Equal(StatusLinkAssinado.Invalido, gerador.Validar("/api/cliente/2/compartilhado", expira, assinatura));
    }

    [Fact]
    public void Validar_ExpiracaoAlterada_DeveRetornarInvalido()
    {
        var gerador = CriarGerador();
        var (caminho, expira, assinatura) = ExtrairPartes(gerador.Gerar(Caminho).Url);
        var expiraEstendida = (long.Parse(expira!) + 86400).ToString();

        Assert.Equal(StatusLinkAssinado.Invalido, gerador.Validar(caminho, expiraEstendida, assinatura));
    }

    [Fact]
    public void Validar_AssinadoComOutraChave_DeveRetornarInvalido()
    {
        var (caminho, expira, assinatura) = ExtrairPartes(CriarGerador().Gerar(Caminho).Url);
        _configuration.Secret = "outra-chave-de-teste-com-tamanho-suficiente-987";

        Assert.Equal(StatusLinkAssinado.Invalido, CriarGerador().Validar(caminho, expira, assinatura));
    }

    [Theory]
    [InlineData(null, "abc")]
    [InlineData("1767272400", null)]
    [InlineData("nao-numerico", "abc")]
    [InlineData("-1", "abc")]
    [InlineData("1767272400", "assinatura@invalida!")]
    public void Validar_ParametrosAusentesOuMalFormados_DeveRetornarInvalido(string? expira, string? assinatura)
    {
        Assert.Equal(StatusLinkAssinado.Invalido, CriarGerador().Validar(Caminho, expira, assinatura));
    }

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public DateTimeOffset Agora { get; set; } = agora;

        public override DateTimeOffset GetUtcNow() => Agora;
    }
}
