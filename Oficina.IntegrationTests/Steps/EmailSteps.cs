using System.Net;
using System.Text.RegularExpressions;
using Oficina.Application.DTOs;
using Oficina.IntegrationTests.Hooks;
using Oficina.IntegrationTests.Support;
using Reqnroll;

namespace Oficina.IntegrationTests.Steps;

/// <summary>
/// E-mails enviados ao cliente e a navegação dele pelos links assinados (sem token JWT).
/// </summary>
[Binding]
public partial class EmailSteps(ApiContext api, OficinaContext oficina)
{
    private static IReadOnlyList<EmailDTO> Enviados => TestRunHooks.Factory.EmailSender.Enviados;

    [When(@"eu enviar o acompanhamento da ordem de serviço ao cliente")]
    public Task QuandoEuEnviarOAcompanhamento() =>
        api.PostAsync($"/api/acompanhamento-ordem-servico/{oficina.IdOs}/envio");

    [When(@"eu reenviar o orçamento da ordem de serviço ao cliente")]
    public Task QuandoEuReenviarOOrcamento() =>
        api.PostAsync($"/api/orcamento/{oficina.IdOs}/envio");

    [Given(@"que o cliente recebeu o e-mail com o assunto ""(.*)""")]
    [Then(@"o cliente deve receber um e-mail com o assunto ""(.*)""")]
    public void EntaoOClienteDeveReceberUmEmail(string assunto) =>
        oficina.Email = Assert.Single(Enviados, email => email.EnderecoDestinatario == oficina.ClienteEmail && email.Assunto == assunto);

    [Then(@"o cliente deve ter recebido (\d+) e-mails? com o assunto ""(.*)""")]
    public void EntaoOClienteDeveTerRecebidoEmails(int quantidade, string assunto)
    {
        var recebidos = Enviados.Where(email => email.EnderecoDestinatario == oficina.ClienteEmail && email.Assunto == assunto).ToList();
        Assert.Equal(quantidade, recebidos.Count);
        oficina.Email = recebidos[^1];
    }

    [Then(@"nenhum e-mail deve ser enviado")]
    public static void EntaoNenhumEmailDeveSerEnviado() => Assert.Empty(Enviados);

    [Then(@"o e-mail deve conter ""(.*)""")]
    public void EntaoOEmailDeveConter(string texto) =>
        Assert.Contains(texto, Conversores.TextoHtml(oficina.Email.CorpoHtml));

    [When(@"o cliente abrir o link ""(.*)"" do e-mail")]
    public Task QuandoOClienteAbrirOLink(string textoBotao)
    {
        oficina.Link = ObterLink(oficina.Email, textoBotao);
        return api.GetAnonimoAsync(oficina.Link);
    }

    [When(@"o cliente abrir o link ""(.*)"" do e-mail com a assinatura adulterada")]
    public Task QuandoOClienteAbrirOLinkAdulterado(string textoBotao)
    {
        var link = ObterLink(oficina.Email, textoBotao);
        return api.GetAnonimoAsync(AssinaturaRegex().Replace(link, "assinatura=adulterada"));
    }

    [When(@"o cliente confirmar a ação na página aberta pelo link")]
    public Task QuandoOClienteConfirmarAAcao() => api.PostAnonimoAsync(oficina.Link);

    [Then(@"a página deve exibir ""(.*)""")]
    public void EntaoAPaginaDeveExibir(string texto)
    {
        Assert.StartsWith("text/html", api.Response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(texto, Conversores.TextoHtml(api.Corpo));
    }

    /// <summary>
    /// URL do botão (âncora) do e-mail pelo texto exibido ao cliente.
    /// </summary>
    private static string ObterLink(EmailDTO email, string textoBotao)
    {
        var link = AncoraRegex().Matches(email.CorpoHtml)
            .Where(ancora => WebUtility.HtmlDecode(ancora.Groups["texto"].Value).Trim() == textoBotao)
            .Select(ancora => WebUtility.HtmlDecode(ancora.Groups["href"].Value))
            .SingleOrDefault();

        Assert.False(string.IsNullOrEmpty(link), $"Botão \"{textoBotao}\" não encontrado no e-mail.");
        return link;
    }

    [GeneratedRegex(@"<a\s+href=""(?<href>[^""]+)""[^>]*>(?<texto>[^<]+)</a>")]
    private static partial Regex AncoraRegex();

    [GeneratedRegex(@"assinatura=[^&]+")]
    private static partial Regex AssinaturaRegex();
}
