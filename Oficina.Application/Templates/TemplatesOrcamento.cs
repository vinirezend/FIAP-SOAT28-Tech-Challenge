using System.Globalization;
using Oficina.Application.DTOs;

namespace Oficina.Application.Templates;

/// <summary>
/// Valores dos templates do orçamento: linhas da tabela do e-mail e as páginas
/// abertas pelo cliente ao clicar em Aprovar/Recusar.
/// </summary>
public static class TemplatesOrcamento
{
    private const string TituloPagina = "Oficina - Orçamento";
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string FormatarMoeda(decimal valor) => valor.ToString("C", PtBr);

    /// <summary>
    /// Linhas &lt;tr&gt; da tabela de itens do orçamento, com os valores já codificados.
    /// </summary>
    public static string GerarLinhasItens(OrcamentoDTO orcamento) => string.Concat(
        orcamento.Servicos.Select(item => GerarLinhaItem("Serviço", item))
            .Concat(orcamento.Insumos.Select(item => GerarLinhaItem("Peça/Insumo", item))));

    private static string GerarLinhaItem(string tipo, OrcamentoItemDTO item) =>
        TemplateHtml.Renderizar(NomesTemplate.Orcamento.EmailItem, new Dictionary<string, string>
        {
            ["tipo"] = tipo,
            ["descricao"] = item.Nome,
            ["quantidade"] = item.Quantidade.ToString(PtBr),
            ["valorUnitario"] = FormatarMoeda(item.ValorUnitario),
            ["valorTotal"] = FormatarMoeda(item.ValorTotal)
        });

    /// <summary>
    /// Página aberta pelo botão do e-mail. A ação só acontece no POST do formulário, para que
    /// leitores de link de antivírus/e-mail (que fazem GET) não aprovem ou recusem sozinhos.
    /// </summary>
    public static string GerarPaginaConfirmacao(int idOs, bool aprovar, decimal valorTotal, string urlAcao) =>
        TemplateHtml.RenderizarPagina(TituloPagina,
            aprovar ? NomesTemplate.Orcamento.PaginaAprovar : NomesTemplate.Orcamento.PaginaRecusar,
            new Dictionary<string, string>
            {
                ["idOs"] = idOs.ToString(),
                ["valorTotal"] = FormatarMoeda(valorTotal),
                ["urlAcao"] = urlAcao
            });

    public static string GerarPaginaMensagem(string titulo, string mensagem) =>
        TemplateHtml.RenderizarPagina(TituloPagina, NomesTemplate.Comum.Mensagem,
            new Dictionary<string, string> { ["titulo"] = titulo, ["mensagem"] = mensagem });
}
