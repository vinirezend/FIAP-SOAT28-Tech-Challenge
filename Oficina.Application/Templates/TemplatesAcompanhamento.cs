using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Oficina.Application.DTOs;
using Oficina.Domain.Enum;

namespace Oficina.Application.Templates;

/// <summary>
/// Textos e página HTML do acompanhamento da OS, aberta pelo cliente via link assinado.
/// </summary>
public static class TemplatesAcompanhamento
{
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);

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

    public static string GerarPagina(AcompanhamentoDTO acompanhamento)
    {
        var etapas = new StringBuilder();
        foreach (var etapa in acompanhamento.Etapas)
            etapas.Append(LinhaEtapa(etapa));

        // O valor só é fechado ao entrar em Aguardando aprovação
        var valor = acompanhamento.IdStatus >= (int)StatusOs.AguardandoAprovacao
            ? $"<p>Valor do orçamento: <strong>{Encoder.Encode(TemplatesOrcamento.FormatarMoeda(acompanhamento.ValorTotal))}</strong></p>"
            : string.Empty;

        return Pagina($"""
            <h2>Ordem de serviço nº {acompanhamento.IdOs}</h2>
            <p>Veículo: {Encoder.Encode($"{acompanhamento.Marca} {acompanhamento.Modelo} ({acompanhamento.Placa})")}</p>
            <div style="background:#e3f2fd;border-radius:4px;padding:12px 16px;margin:16px 0">
                <p style="margin:0 0 4px"><strong>{Encoder.Encode(acompanhamento.Status ?? "Sem status")}</strong></p>
                <p style="margin:0">{Encoder.Encode(acompanhamento.DescricaoStatus ?? string.Empty)}</p>
            </div>
            {valor}
            <ol style="list-style:none;padding:0;margin:24px 0">{etapas}</ol>
            <p style="font-size:12px;color:#777">Horários de Brasília. Atualize a página para ver o andamento mais recente.</p>
            """);
    }

    public static string GerarPaginaMensagem(string titulo, string mensagem) =>
        Pagina($"<h2>{Encoder.Encode(titulo)}</h2><p>{Encoder.Encode(mensagem)}</p>");

    private static string LinhaEtapa(AcompanhamentoEtapaDTO etapa)
    {
        var (icone, cor, peso) = etapa.Situacao switch
        {
            SituacaoEtapa.Concluida => ("&#10003;", "#2e7d32", "normal"),
            SituacaoEtapa.Atual => ("&#9679;", "#1565c0", "bold"),
            SituacaoEtapa.NaoRealizada => ("&#8211;", "#9e9e9e", "normal"),
            _ => ("&#9675;", "#9e9e9e", "normal")
        };

        var data = etapa.Data is { } dataEtapa
            ? HorarioBrasilia.ConverterDeUtc(dataEtapa).ToString("dd/MM/yyyy HH:mm")
            : etapa.Situacao == SituacaoEtapa.NaoRealizada ? "Não realizada" : string.Empty;

        return $"""
            <li style="padding:8px 0;border-bottom:1px solid #eee;color:{cor};font-weight:{peso}">
                <span style="display:inline-block;width:24px">{icone}</span>{Encoder.Encode(etapa.Nome)}
                <span style="float:right;font-weight:normal;color:#777">{Encoder.Encode(data)}</span>
            </li>
            """;
    }

    private static string Pagina(string conteudo) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Oficina - Acompanhamento</title></head>
        <body style="font-family:Arial,sans-serif;max-width:600px;margin:40px auto;padding:0 16px;color:#333">
        {conteudo}
        </body>
        </html>
        """;
}
