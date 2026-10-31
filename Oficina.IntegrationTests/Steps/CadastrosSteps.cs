using Oficina.Domain.Enum;
using Oficina.IntegrationTests.Support;
using Reqnroll;

namespace Oficina.IntegrationTests.Steps;

/// <summary>
/// Pré-condições dos fluxos de OS: colaborador autenticado e cadastros de cliente, veículo, serviço e insumo.
/// </summary>
[Binding]
public class CadastrosSteps(ApiContext api, OficinaContext oficina)
{
    private const string SenhaPadrao = "senha-teste-123";

    [Given(@"que estou autenticado como o colaborador ""(.*)""")]
    public async Task DadoQueEstouAutenticadoComoColaborador(string login)
    {
        oficina.IdColaborador = await OficinaSeeder.CriarColaboradorAsync(login, SenhaPadrao, Departamento.Recepcao);

        await api.PostAsync("/api/login/signin", new { login, senha = SenhaPadrao });
        Assert.True(api.Response.IsSuccessStatusCode, $"Falha no login: {(int)api.Response.StatusCode}");

        api.Autenticar(api.Corpo.Trim('"'));
    }

    [Given(@"que o cliente ""(.*)"" com e-mail ""(.*)"" possui o veículo ""(.*)"" da marca ""(.*)"" de placa ""(.*)""")]
    public async Task DadoQueOClientePossuiOVeiculo(string nome, string email, string modelo, string marca, string placa)
    {
        var idCliente = await OficinaSeeder.CriarClienteAsync(nome, "32887790056", email);

        oficina.Veiculos[placa] = await OficinaSeeder.CriarVeiculoAsync(idCliente, placa, marca, modelo);
        oficina.ClienteEmail = email;
    }

    [Given(@"que existe o serviço ""(.*)"" no valor de R\$ (.*)")]
    public async Task DadoQueExisteOServico(string nome, string valor) =>
        oficina.Servicos[nome] = await OficinaSeeder.CriarServicoAsync(nome, Conversores.Valor(valor));

    [Given(@"que existe o insumo ""(.*)"" no valor de R\$ (.*) com (\d+) unidades em estoque")]
    public async Task DadoQueExisteOInsumo(string nome, string valor, int estoque) =>
        oficina.Insumos[nome] = await OficinaSeeder.CriarInsumoAsync(nome, Conversores.Valor(valor), estoque);
}
