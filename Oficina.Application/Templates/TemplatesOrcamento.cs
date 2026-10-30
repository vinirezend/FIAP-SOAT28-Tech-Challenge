using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Oficina.Application.DTOs;

namespace Oficina.Application.Templates;

/// <summary>
/// Trechos e páginas HTML do orçamento: linhas da tabela do e-mail e as páginas
/// abertas pelo cliente ao clicar em Aprovar/Recusar.
/// </summary>
public static class TemplatesOrcamento
{
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string FormatarMoeda(decimal valor) => valor.ToString("C", PtBr);

    /// <summary>
    /// Linhas &lt;tr&gt; da tabela de itens do orçamento, com os valores já codificados.
    /// </summary>
    public static string GerarLinhasItens(OrcamentoDTO orcamento)
    {
        var html = new StringBuilder();
        AdicionarLinhas(html, "Serviço", orcamento.Servicos);
        AdicionarLinhas(html, "Peça/Insumo", orcamento.Insumos);
        return html.ToString();
    }

    private static void AdicionarLinhas(StringBuilder html, string tipo, IEnumerable<OrcamentoItemDTO> itens)
    {
        foreach (var item in itens)
        {
            html.Append("<tr>")
                .Append(Celula(tipo))
                .Append(Celula(item.Nome))
                .Append(Celula(item.Quantidade.ToString(PtBr), "center"))
                .Append(Celula(FormatarMoeda(item.ValorUnitario), "right"))
                .Append(Celula(FormatarMoeda(item.ValorTotal), "right"))
                .Append("</tr>");
        }
    }

    private static string Celula(string valor, string alinhamento = "left") =>
        $"<td style=\"padding:8px;border-bottom:1px solid #ddd;text-align:{alinhamento}\">{Encoder.Encode(valor)}</td>";

    /// <summary>
    /// Página aberta pelo botão do e-mail. A ação só acontece no POST do formulário, para que
    /// leitores de link de antivírus/e-mail (que fazem GET) não aprovem ou recusem sozinhos.
    /// </summary>
    public static string GerarPaginaConfirmacao(int idOs, bool aprovar, decimal valorTotal, string urlAcao)
    {
        var acao = aprovar ? "aprovar" : "recusar";
        var cor = aprovar ? "#2e7d32" : "#c62828";
        var detalhe = aprovar
            ? "Ao aprovar, a oficina iniciará a execução dos serviços."
            : "Ao recusar, a OS será finalizada sem a execução dos serviços.";

        return Pagina($"""
            <h2>Ordem de serviço nº {idOs}</h2>
            <p>Deseja <strong>{acao}</strong> o orçamento no valor de <strong>{Encoder.Encode(FormatarMoeda(valorTotal))}</strong>?</p>
            <p>{detalhe}</p>
            <form method="post" action="{Encoder.Encode(urlAcao)}">
                <button type="submit" style="background:{cor};color:#fff;border:0;border-radius:4px;padding:12px 24px;font-size:16px;cursor:pointer">Confirmar</button>
            </form>
            """);
    }

    public static string GerarPaginaMensagem(string titulo, string mensagem) =>
        Pagina($"<h2>{Encoder.Encode(titulo)}</h2><p>{Encoder.Encode(mensagem)}</p>");

    private static string Pagina(string conteudo) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Oficina - Orçamento</title></head>
        <body style="font-family:Arial,sans-serif;max-width:600px;margin:40px auto;padding:0 16px;color:#333">
        {conteudo}
        </body>
        </html>
        """;
}
