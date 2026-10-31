using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Templates;

namespace Oficina.Api.ErrorHandling;

/// <summary>
/// Respostas HTML das páginas abertas pelo cliente a partir dos links do e-mail (orçamento e acompanhamento).
/// </summary>
public static class PaginaHtml
{
    private const string TituloPadrao = "Oficina";

    public static ContentResult Criar(string conteudo, int statusCode = StatusCodes.Status200OK) => new()
    {
        Content = conteudo,
        ContentType = "text/html; charset=utf-8",
        StatusCode = statusCode
    };

    /// <summary>
    /// Página simples de aviso (título + mensagem), usada quando ainda não se sabe de qual fluxo é o link.
    /// </summary>
    public static ContentResult Mensagem(string titulo, string mensagem, int statusCode) =>
        Criar(TemplateHtml.RenderizarPagina(TituloPadrao, NomesTemplate.Comum.Mensagem,
            new Dictionary<string, string> { ["titulo"] = titulo, ["mensagem"] = mensagem }), statusCode);
}
