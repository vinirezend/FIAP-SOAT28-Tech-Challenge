using Oficina.Application.Templates;

namespace Oficina.Tests.Application.Templates;

public class TemplatesOrcamentoTests
{
    [Theory]
    [InlineData(true, "aprovar", "botao-aprovar")]
    [InlineData(false, "recusar", "botao-recusar")]
    public void GerarPaginaConfirmacao_DeveMontarFormularioComPostNoLinkAssinado(bool aprovar, string acao, string classeBotao)
    {
        var html = TemplatesOrcamento.GerarPaginaConfirmacao(1, aprovar, 331.6m, "/api/orcamento/1/aprovar?expira=1&assinatura=abc");

        Assert.Contains("Ordem de serviço nº 1", html);
        Assert.Contains($"<strong>{acao}</strong>", html);
        Assert.Contains(TemplatesOrcamento.FormatarMoeda(331.6m), html);
        Assert.Contains("<form method=\"post\" action=\"/api/orcamento/1/aprovar?expira=1&amp;assinatura=abc\">", html);
        Assert.Contains(classeBotao, html);
        Assert.DoesNotContain("{{", html);
    }

    [Fact]
    public void GerarPaginaMensagem_DeveCodificarTituloEMensagem()
    {
        var html = TemplatesOrcamento.GerarPaginaMensagem("Erro <x>", "Falha & motivo");

        Assert.Contains("<title>Oficina - Orçamento</title>", html);
        Assert.Contains("<h2>Erro &lt;x&gt;</h2>", html);
        Assert.Contains("<p>Falha &amp; motivo</p>", html);
    }
}
