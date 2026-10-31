using Oficina.Application.Templates;

namespace Oficina.Tests.Application.Templates;

public class TemplateHtmlTests
{
    [Fact]
    public void Preencher_DeveSubstituirPlaceholdersIgnorandoEspacosEMaiusculas()
    {
        var html = TemplateHtml.Preencher(
            "<p>OS {{ numeroOs }} de {{NOME}}</p>",
            new Dictionary<string, string> { ["nome"] = "Ana", ["numeroOs"] = "123" });

        Assert.Equal("<p>OS 123 de Ana</p>", html);
    }

    [Fact]
    public void Preencher_DeveCodificarValoresEInserirTrechosHtmlSemCodificar()
    {
        var html = TemplateHtml.Preencher(
            "<p>{{nome}}</p><table>{{itens}}</table>",
            new Dictionary<string, string> { ["nome"] = "Ana <Silva> & \"Cia\"" },
            new Dictionary<string, string> { ["itens"] = "<tr><td>Óleo</td></tr>" });

        Assert.Equal("<p>Ana &lt;Silva&gt; &amp; &quot;Cia&quot;</p><table><tr><td>Óleo</td></tr></table>", html);
    }

    [Fact]
    public void Preencher_PlaceholderSemValor_DeveLancarExcecao()
    {
        var excecao = Assert.Throws<InvalidOperationException>(
            () => TemplateHtml.Preencher("<p>{{nome}}</p>", new Dictionary<string, string>()));

        Assert.Contains("{{nome}}", excecao.Message);
    }

    [Fact]
    public void Renderizar_TemplateInexistente_DeveLancarExcecao()
    {
        var excecao = Assert.Throws<InvalidOperationException>(
            () => TemplateHtml.Renderizar("nao-existe", new Dictionary<string, string>()));

        Assert.Contains("nao-existe", excecao.Message);
    }

    public static TheoryData<string> NomesTemplates() => new(typeof(NomesTemplate)
        .GetNestedTypes()
        .SelectMany(contexto => contexto.GetFields())
        .Where(campo => campo.IsLiteral)
        .Select(campo => (string)campo.GetRawConstantValue()!));

    [Theory]
    [MemberData(nameof(NomesTemplates))]
    public void NomesTemplate_CadaCaminhoDeveApontarParaUmArquivoEmbutido(string nome)
    {
        var recursos = typeof(TemplateHtml).Assembly.GetManifestResourceNames().Select(recurso => recurso.Replace('\\', '/'));

        Assert.Contains($"Templates/{nome}.html", recursos);
    }

    [Fact]
    public void RenderizarPagina_DeveInserirConteudoNoLayout()
    {
        var html = TemplateHtml.RenderizarPagina("Oficina - Teste", NomesTemplate.Comum.Mensagem,
            new Dictionary<string, string> { ["titulo"] = "Título", ["mensagem"] = "Mensagem <b>" });

        Assert.StartsWith("<!DOCTYPE html>", html);
        Assert.Contains("<title>Oficina - Teste</title>", html);
        Assert.Contains("<h2>Título</h2>", html);
        Assert.Contains("<p>Mensagem &lt;b&gt;</p>", html);
        Assert.DoesNotContain("{{", html);
    }
}
