using Oficina.Application.DTOs;
using Oficina.Domain.Enum;

namespace Oficina.Application.Templates;

/// <summary>
/// Textos e valores da página de acompanhamento da ordem de serviço, aberta pelo cliente via link assinado.
/// </summary>
public static class TemplatesAcompanhamentoOrdemServico
{
    private const string TituloPagina = "Oficina - Acompanhamento da ordem de serviço";

    public const string DescricaoOrcamentoRecusado =
        "O orçamento foi recusado e a ordem de serviço foi finalizada sem a execução dos serviços. Seu veículo está disponível para retirada.";

    public static string NomeStatus(StatusOs status) => status switch
    {
        StatusOs.Recebida => "Recebida",
        StatusOs.EmDiagnostico => "Em diagnóstico",
        StatusOs.AguardandoAprovacao => "Aguardando aprovação",
        StatusOs.EmExecucao => "Em execução",
        StatusOs.Finalizada => "Finalizada",
        StatusOs.Entregue => "Entregue",
        _ => status.ToString()
    };

    public static string DescricaoStatus(StatusOs status) => status switch
    {
        StatusOs.Recebida => "Recebemos seu veículo e a ordem de serviço foi aberta.",
        StatusOs.EmDiagnostico => "Nossa equipe está avaliando o veículo para montar o orçamento.",
        StatusOs.AguardandoAprovacao => "O orçamento foi enviado para o seu e-mail e aguarda sua aprovação.",
        StatusOs.EmExecucao => "O orçamento foi aprovado e os serviços estão sendo executados.",
        StatusOs.Finalizada => "Os serviços foram concluídos. Seu veículo está pronto para retirada.",
        StatusOs.Entregue => "O veículo foi entregue. Obrigado pela preferência!",
        _ => string.Empty
    };

    public static string GerarPagina(AcompanhamentoOrdemServicoDTO acompanhamento)
    {
        // O valor só é fechado ao entrar em Aguardando aprovação
        var valor = acompanhamento.IdStatus >= (int)StatusOs.AguardandoAprovacao
            ? TemplateHtml.Renderizar(NomesTemplate.AcompanhamentoOrdemServico.PaginaValor, new Dictionary<string, string>
            {
                ["valorTotal"] = TemplatesOrcamento.FormatarMoeda(acompanhamento.ValorTotal)
            })
            : string.Empty;

        return TemplateHtml.RenderizarPagina(TituloPagina, NomesTemplate.AcompanhamentoOrdemServico.Pagina,
            new Dictionary<string, string>
            {
                ["idOs"] = acompanhamento.IdOs.ToString(),
                ["veiculo"] = $"{acompanhamento.Marca} {acompanhamento.Modelo} ({acompanhamento.Placa})",
                ["status"] = acompanhamento.Status ?? "Sem status",
                ["descricaoStatus"] = acompanhamento.DescricaoStatus ?? string.Empty
            },
            new Dictionary<string, string>
            {
                ["valor"] = valor,
                ["etapas"] = string.Concat(acompanhamento.Etapas.Select(GerarEtapa))
            });
    }

    public static string GerarPaginaMensagem(string titulo, string mensagem) =>
        TemplateHtml.RenderizarPagina(TituloPagina, NomesTemplate.Comum.Mensagem,
            new Dictionary<string, string> { ["titulo"] = titulo, ["mensagem"] = mensagem });

    private static string GerarEtapa(AcompanhamentoOrdemServicoEtapaDTO etapa)
    {
        var data = etapa.Data is { } dataEtapa
            ? HorarioBrasilia.ConverterDeUtc(dataEtapa).ToString("dd/MM/yyyy HH:mm")
            : etapa.Situacao == SituacaoEtapa.NaoRealizada ? "Não realizada" : string.Empty;

        return TemplateHtml.Renderizar(NomesTemplate.AcompanhamentoOrdemServico.PaginaEtapa, new Dictionary<string, string>
        {
            // Classe CSS etapa-{situacao} definida em Comum/layout.html
            ["situacao"] = etapa.Situacao.ToString().ToLowerInvariant(),
            ["nome"] = etapa.Nome,
            ["data"] = data
        });
    }
}
