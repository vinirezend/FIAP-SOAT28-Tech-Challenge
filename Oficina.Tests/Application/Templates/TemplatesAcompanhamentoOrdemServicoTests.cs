using Oficina.Application.DTOs;
using Oficina.Application.Templates;
using Oficina.Domain.Enum;

namespace Oficina.Tests.Application.Templates;

public class TemplatesAcompanhamentoOrdemServicoTests
{
    private static AcompanhamentoOrdemServicoDTO Acompanhamento(StatusOs status, string modelo = "Gol") => new()
    {
        IdOs = 1,
        IdStatus = (int)status,
        Status = TemplatesAcompanhamentoOrdemServico.NomeStatus(status),
        DescricaoStatus = TemplatesAcompanhamentoOrdemServico.DescricaoStatus(status),
        ClienteNome = "Lucas Ferreira",
        ClienteEmail = "lucas@example.com",
        Placa = "FFF2026",
        Marca = "Volkswagen",
        Modelo = modelo,
        Ano = 2019,
        ValorTotal = 331.6m,
        Etapas =
        [
            new() { IdStatus = 1, Nome = "Recebida", Situacao = SituacaoEtapa.Concluida, Data = new DateTime(2026, 10, 27, 12, 0, 0) },
            new() { IdStatus = 2, Nome = "Em diagnóstico", Situacao = SituacaoEtapa.Atual, Data = new DateTime(2026, 10, 28, 13, 0, 0) },
            new() { IdStatus = 3, Nome = "Aguardando aprovação", Situacao = SituacaoEtapa.Pendente }
        ]
    };

    [Fact]
    public void GerarPagina_DeveExibirStatusEtapasEDatasNoHorarioDeBrasilia()
    {
        var html = TemplatesAcompanhamentoOrdemServico.GerarPagina(Acompanhamento(StatusOs.EmDiagnostico));

        Assert.Contains("Ordem de serviço nº 1", html);
        Assert.Contains("Volkswagen Gol (FFF2026)", html);
        Assert.Contains("Em diagnóstico", html);
        Assert.Contains("Aguardando aprovação", html);
        // 12:00 UTC -> 09:00 em Brasília
        Assert.Contains("27/10/2026 09:00", html);
    }

    [Fact]
    public void GerarPagina_AntesDoOrcamento_NaoDeveExibirValor()
    {
        var html = TemplatesAcompanhamentoOrdemServico.GerarPagina(Acompanhamento(StatusOs.EmDiagnostico));

        Assert.DoesNotContain("Valor do orçamento", html);
    }

    [Fact]
    public void GerarPagina_AposOrcamento_DeveExibirValor()
    {
        var html = TemplatesAcompanhamentoOrdemServico.GerarPagina(Acompanhamento(StatusOs.AguardandoAprovacao));

        Assert.Contains("Valor do orçamento", html);
        Assert.Contains(TemplatesOrcamento.FormatarMoeda(331.6m), html);
    }

    [Fact]
    public void GerarPagina_DadosComHtml_DeveCodificar()
    {
        var html = TemplatesAcompanhamentoOrdemServico.GerarPagina(Acompanhamento(StatusOs.EmDiagnostico, "<script>x</script>"));

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }
}
