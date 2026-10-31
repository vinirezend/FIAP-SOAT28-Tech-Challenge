using System.Net;
using Oficina.Api.DTO.OrdemServico;
using Oficina.Domain.Enum;
using Oficina.IntegrationTests.Support;
using Reqnroll;

namespace Oficina.IntegrationTests.Steps;

[Binding]
public class OrdemServicoSteps(ApiContext api, OficinaContext oficina)
{
    private const string OrdemServicoUrl = "/api/ordem-servico";

    private string UrlOs => $"{OrdemServicoUrl}/{oficina.IdOs}";

    [Given(@"que existe uma ordem de serviço aberta para o veículo de placa ""(.*)""")]
    public async Task DadoQueExisteUmaOrdemDeServicoAberta(string placa)
    {
        await QuandoEuAbrirUmaOrdemDeServico(placa);
        Assert.Equal(HttpStatusCode.Created, api.Response.StatusCode);
    }

    [Given(@"que a ordem de serviço está com status ""(.*)""")]
    public async Task DadoQueAOrdemDeServicoEstaComStatus(string nomeStatus)
    {
        var destino = Conversores.Status(nomeStatus);

        // O fluxo só avança um status por vez
        for (var status = StatusOs.EmDiagnostico; status <= destino; status++)
        {
            await AlterarStatusAsync(status);
            Assert.True(api.Response.IsSuccessStatusCode, $"Falha ao avançar para {status}: {api.Corpo}");
        }
    }

    [Given(@"que o serviço ""(.*)"" foi incluído na ordem de serviço com quantidade (\d+)")]
    [When(@"eu incluir o serviço ""(.*)"" na ordem de serviço com quantidade (\d+)")]
    public async Task QuandoEuIncluirOServico(string nome, int quantidade)
    {
        await api.PostAsync($"{UrlOs}/servicos", new AdicionarServicoRequest { IdServico = oficina.Servicos[nome], Quantidade = quantidade });
        Assert.Equal(HttpStatusCode.Created, api.Response.StatusCode);
    }

    [Given(@"que o insumo ""(.*)"" foi incluído na ordem de serviço com quantidade (\d+)")]
    [When(@"eu incluir o insumo ""(.*)"" na ordem de serviço com quantidade (\d+)")]
    public async Task QuandoEuIncluirOInsumo(string nome, int quantidade)
    {
        await api.PostAsync($"{UrlOs}/insumos", new AdicionarInsumoRequest { IdInsumo = oficina.Insumos[nome], Quantidade = quantidade });
        Assert.Equal(HttpStatusCode.Created, api.Response.StatusCode);
    }

    [When(@"eu abrir uma ordem de serviço para o veículo de placa ""(.*)""")]
    public async Task QuandoEuAbrirUmaOrdemDeServico(string placa)
    {
        await api.PostAsync(OrdemServicoUrl, new OrdemServicoRequest { IdVeiculo = oficina.Veiculos[placa] });

        if (api.Response.StatusCode == HttpStatusCode.Created)
            oficina.IdOs = api.Ler<OrdemServicoResponse>().IdOs;
    }

    [When(@"eu tentar abrir uma ordem de serviço sem estar autenticado")]
    public async Task QuandoEuTentarAbrirSemAutenticacao()
    {
        api.Client.DefaultRequestHeaders.Authorization = null;
        await api.PostAsync(OrdemServicoUrl, new OrdemServicoRequest { IdVeiculo = oficina.Veiculos.Values.First() });
    }

    [When(@"eu alterar o status da ordem de serviço para ""(.*)""")]
    public Task QuandoEuAlterarOStatus(string nomeStatus) => AlterarStatusAsync(Conversores.Status(nomeStatus));

    [Then(@"a ordem de serviço deve ser criada para o veículo de placa ""(.*)"" com status ""(.*)""")]
    public void EntaoAOrdemDeServicoDeveSerCriada(string placa, string nomeStatus)
    {
        Assert.Equal(HttpStatusCode.Created, api.Response.StatusCode);
        Assert.Equal($"{OrdemServicoUrl}/{oficina.IdOs}", api.Response.Headers.Location?.OriginalString, ignoreCase: true);

        var criada = api.Ler<OrdemServicoResponse>();
        Assert.True(criada.IdOs > 0);
        Assert.Equal(oficina.Veiculos[placa], criada.IdVeiculo);
        Assert.Equal((int)Conversores.Status(nomeStatus), criada.IdStatus);
        Assert.Equal(0m, criada.Valor);
        Assert.Null(criada.DataInicio);
        Assert.Null(criada.DataFim);
    }

    [Then(@"a ordem de serviço deve estar com status ""(.*)""")]
    public async Task EntaoAOrdemDeServicoDeveEstarComStatus(string nomeStatus)
    {
        var ordemServico = await ObterOrdemServicoAsync();
        Assert.Equal((int)Conversores.Status(nomeStatus), ordemServico.IdStatus);
    }

    [Then(@"a ordem de serviço deve ter o valor de R\$ (.*)")]
    public async Task EntaoAOrdemDeServicoDeveTerOValor(string valor)
    {
        var ordemServico = await ObterOrdemServicoAsync();
        Assert.Equal(Conversores.Valor(valor), ordemServico.Valor);
    }

    [Then(@"a ordem de serviço deve ter a data de início da execução preenchida")]
    public async Task EntaoAOrdemDeServicoDeveTerDataInicio() =>
        Assert.NotNull((await ObterOrdemServicoAsync()).DataInicio);

    [Then(@"a ordem de serviço deve ter a data de fim preenchida sem data de início da execução")]
    public async Task EntaoAOrdemDeServicoDeveTerDataFimSemInicio()
    {
        var ordemServico = await ObterOrdemServicoAsync();
        Assert.Null(ordemServico.DataInicio);
        Assert.NotNull(ordemServico.DataFim);
    }

    [Then(@"o histórico da ordem de serviço deve ter os status ""(.*)"" registrados pelo colaborador autenticado")]
    public async Task EntaoOHistoricoDeveTerOsStatus(string nomesStatus)
    {
        await api.GetAsync($"{UrlOs}/status");
        var historico = api.Ler<List<OrdemServicoStatusResponse>>();

        var esperados = nomesStatus.Split(',', StringSplitOptions.TrimEntries).Select(nome => (int)Conversores.Status(nome));
        Assert.Equal(esperados, historico.Select(h => h.IdStatus));
        Assert.All(historico, h => Assert.Equal(oficina.IdColaborador, h.IdColaborador));
    }

    [Then(@"o insumo ""(.*)"" deve ter (\d+) unidades reservadas")]
    public async Task EntaoOInsumoDeveTerUnidadesReservadas(string nome, int quantidade) =>
        Assert.Equal(quantidade, await OficinaSeeder.ObterQuantidadeReservadaAsync(oficina.Insumos[nome]));

    private Task AlterarStatusAsync(StatusOs status) =>
        api.PostAsync($"{UrlOs}/status", new AlterarStatusRequest { IdStatus = (int)status });

    private async Task<OrdemServicoResponse> ObterOrdemServicoAsync()
    {
        await api.GetAsync(UrlOs);
        return api.Ler<OrdemServicoResponse>();
    }
}
