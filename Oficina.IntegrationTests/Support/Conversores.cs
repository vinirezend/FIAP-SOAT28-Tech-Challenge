using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Oficina.Application.Templates;
using Oficina.Domain.Enum;

namespace Oficina.IntegrationTests.Support;

/// <summary>
/// Converte os textos usados nas features (nomes de status, valores em reais, HTML) para os tipos da API.
/// </summary>
public static partial class Conversores
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>
    /// Status pelo nome exibido ao cliente (ex.: "Em diagnóstico").
    /// </summary>
    public static StatusOs Status(string nome)
    {
        foreach (var status in Enum.GetValues<StatusOs>())
            if (TemplatesAcompanhamentoOrdemServico.NomeStatus(status) == nome)
                return status;

        throw new ArgumentException($"Status desconhecido: {nome}");
    }

    public static decimal Valor(string valor) => decimal.Parse(valor, NumberStyles.Number, PtBr);

    /// <summary>
    /// Texto visível do HTML: sem tags e entidades, com espaços normalizados (inclusive o não separável de "R$ 10,00").
    /// </summary>
    public static string TextoHtml(string html)
    {
        var texto = WebUtility.HtmlDecode(TagRegex().Replace(html, string.Empty));
        return EspacosRegex().Replace(texto, " ");
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex TagRegex();

    // \s cobre o espaço não separável (U+00A0)
    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacosRegex();
}
