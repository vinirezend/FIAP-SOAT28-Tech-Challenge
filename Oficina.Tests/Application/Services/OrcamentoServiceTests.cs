using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Implementations.Services;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.Templates;
using Oficina.Domain.Entities;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositories;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Application.Services;

public class OrcamentoServiceTests
{
    private const int IdColaboradorAprovacao = 4;
    private static readonly DateTimeOffset Agora = new(2026, 10, 29, 15, 30, 0, TimeSpan.Zero);

    private readonly Mock<IOrdemServicoRepository> _ordemServicoRepositoryMock = new();
    private readonly Mock<IVeiculoRepository> _veiculoRepositoryMock = new();
    private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
    private readonly Mock<IServicoRepository> _servicoRepositoryMock = new();
    private readonly Mock<IInsumoRepository> _insumoRepositoryMock = new();
    private readonly Mock<IEnvioLinkAssinadoService> _envioLinkAssinadoServiceMock = new();
    private readonly Mock<TimeProvider> _timeProviderMock = new();
    private readonly OrcamentoService _service;

    private EnvioLinkAssinadoDTO? _envio;

    public OrcamentoServiceTests()
    {
        _timeProviderMock.Setup(t => t.GetUtcNow()).Returns(Agora);

        _veiculoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Veiculo.Reconstituir(3, 9, Placa.Criar("FFF2026"), "Gol", "Volkswagen", 2019, TipoVeiculo.Carro));
        _clienteRepositoryMock
            .Setup(r => r.ObterPorIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Cliente.Reconstituir(9, "Lucas <Ferreira>", Documento.Criar("32887790056"), Email.Criar("lucas@example.com"), Telefone.Criar("(19) 98888-8888")));

        _ordemServicoRepositoryMock
            .Setup(r => r.ObterServicosAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([OrdemServicoServico.Reconstituir(10, 1, 5, 2, 80m)]);
        _ordemServicoRepositoryMock
            .Setup(r => r.ObterInsumosAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([OrdemServicoInsumo.Reconstituir(11, 1, 7, 4, 42.9m)]);
        _servicoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Servico.Reconstituir(5, "Troca de óleo", "Descrição", 80m, true));
        _insumoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Insumo.Reconstituir(7, "Óleo 5W30", "Descrição", 42.9m, true, 40, 4));

        _ordemServicoRepositoryMock
            .Setup(r => r.ObterHistoricoStatusAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                OrdemServicoStatus.Reconstituir(1, 1, StatusOs.Recebida, 2, Agora.DateTime),
                OrdemServicoStatus.Reconstituir(2, 1, StatusOs.AguardandoAprovacao, IdColaboradorAprovacao, Agora.DateTime)
            ]);

        _envioLinkAssinadoServiceMock
            .Setup(s => s.EnviarAsync(It.IsAny<EnvioLinkAssinadoDTO>(), It.IsAny<CancellationToken>()))
            .Callback<EnvioLinkAssinadoDTO, CancellationToken>((envio, _) => _envio = envio)
            .ReturnsAsync(new LinkAssinadoDTO { ExpiraEm = Agora });

        _service = new OrcamentoService(
            _ordemServicoRepositoryMock.Object,
            _veiculoRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _servicoRepositoryMock.Object,
            _insumoRepositoryMock.Object,
            _envioLinkAssinadoServiceMock.Object,
            _timeProviderMock.Object);
    }

    private void ConfigurarOs(StatusOs? status) =>
        _ordemServicoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrdemServico.Reconstituir(1, 3, null, null, 331.6m, status));

    [Fact]
    public async Task ObterAsync_DeveMontarOrcamentoComClienteVeiculoEItens()
    {
        ConfigurarOs(StatusOs.AguardandoAprovacao);

        var orcamento = await _service.ObterAsync(1);

        Assert.NotNull(orcamento);
        Assert.Equal("Lucas <Ferreira>", orcamento.ClienteNome);
        Assert.Equal("lucas@example.com", orcamento.ClienteEmail);
        Assert.Equal("FFF2026", orcamento.Placa);
        Assert.Equal(331.6m, orcamento.ValorTotal);
        var servico = Assert.Single(orcamento.Servicos);
        Assert.Equal("Troca de óleo", servico.Nome);
        Assert.Equal(160m, servico.ValorTotal);
        var insumo = Assert.Single(orcamento.Insumos);
        Assert.Equal("Óleo 5W30", insumo.Nome);
        Assert.Equal(4, insumo.Quantidade);
        Assert.Equal(171.6m, insumo.ValorTotal);
    }

    [Fact]
    public async Task ObterAsync_OsInexistente_DeveRetornarNull()
    {
        Assert.Null(await _service.ObterAsync(99));
    }

    [Fact]
    public async Task EnviarAsync_AguardandoAprovacao_DeveEnviarEmailComTabelaEBotoesAssinados()
    {
        ConfigurarOs(StatusOs.AguardandoAprovacao);

        var resultado = await _service.EnviarAsync(1, 60, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.NotNull(_envio);
        Assert.Equal("lucas@example.com", _envio.EmailDestinatario);
        Assert.Equal(TemplatesEmail.OrcamentoAssunto, _envio.Assunto);
        Assert.Equal(NomesTemplate.Orcamento.Email, _envio.Template);
        Assert.Equal(60, _envio.MinutosValidade);
        Assert.Null(_envio.Caminho);
        Assert.Equal("/api/orcamento/1/aprovar", _envio.LinksAssinados["aprovar"]);
        Assert.Equal("/api/orcamento/1/recusar", _envio.LinksAssinados["recusar"]);
        Assert.Equal("Lucas <Ferreira>", _envio.Parametros["nome"]);
        Assert.Equal("Volkswagen Gol (FFF2026)", _envio.Parametros["veiculo"]);
        Assert.Equal(TemplatesOrcamento.FormatarMoeda(331.6m), _envio.Parametros["valorTotal"]);

        var itens = _envio.ParametrosHtml["itens"];
        Assert.Contains("Troca de óleo", itens);
        Assert.Contains("Óleo 5W30", itens);
        Assert.Contains(TemplatesOrcamento.FormatarMoeda(171.6m), itens);
    }

    [Fact]
    public async Task EnviarAsync_NomeDoItemComHtml_DeveCodificarNaTabela()
    {
        ConfigurarOs(StatusOs.AguardandoAprovacao);
        _servicoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Servico.Reconstituir(5, "<script>x</script>", "Descrição", 80m, true));

        await _service.EnviarAsync(1, null, CancellationToken.None);

        Assert.DoesNotContain("<script>", _envio!.ParametrosHtml["itens"]);
        Assert.Contains("&lt;script&gt;", _envio.ParametrosHtml["itens"]);
    }

    [Theory]
    [InlineData(StatusOs.Recebida)]
    [InlineData(StatusOs.EmDiagnostico)]
    [InlineData(StatusOs.EmExecucao)]
    public async Task EnviarAsync_ForaDeAguardandoAprovacao_DeveLancarExcecaoSemEnviar(StatusOs status)
    {
        ConfigurarOs(status);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.EnviarAsync(1, null, CancellationToken.None));
        _envioLinkAssinadoServiceMock.Verify(s => s.EnviarAsync(It.IsAny<EnvioLinkAssinadoDTO>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnviarAsync_OsInexistente_DeveRetornarNull()
    {
        Assert.Null(await _service.EnviarAsync(99, null, CancellationToken.None));
    }

    [Fact]
    public async Task AprovarAsync_DeveGravarEmExecucaoEmNomeDoColaboradorQuePosEmAprovacao()
    {
        ConfigurarOs(StatusOs.AguardandoAprovacao);

        var resultado = await _service.AprovarAsync(1);

        Assert.Equal((int)StatusOs.EmExecucao, resultado!.IdStatus);
        Assert.Equal(Agora.DateTime, resultado.DataInicio);
        _ordemServicoRepositoryMock.Verify(r => r.AlterarStatusAsync(
            It.Is<OrdemServico>(os => os.Status == StatusOs.EmExecucao),
            IdColaboradorAprovacao,
            Agora.DateTime, It.IsAny<CancellationToken>()), Times.Once);
        _insumoRepositoryMock.Verify(r => r.RetirarReservaAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecusarAsync_DeveLiberarReservasEFinalizarOs()
    {
        ConfigurarOs(StatusOs.AguardandoAprovacao);

        var resultado = await _service.RecusarAsync(1);

        Assert.Equal((int)StatusOs.Finalizada, resultado!.IdStatus);
        Assert.Equal(Agora.DateTime, resultado.DataFim);
        _insumoRepositoryMock.Verify(r => r.RetirarReservaAsync(7, 4, It.IsAny<CancellationToken>()), Times.Once);
        _ordemServicoRepositoryMock.Verify(r => r.AlterarStatusAsync(
            It.Is<OrdemServico>(os => os.Status == StatusOs.Finalizada),
            IdColaboradorAprovacao,
            Agora.DateTime, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecusarAsync_OrcamentoJaRespondido_NaoDeveLiberarReservasNemGravar()
    {
        ConfigurarOs(StatusOs.EmExecucao);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RecusarAsync(1));
        _insumoRepositoryMock.Verify(r => r.RetirarReservaAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _ordemServicoRepositoryMock.Verify(r => r.AlterarStatusAsync(It.IsAny<OrdemServico>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AprovarAsync_OsInexistente_DeveRetornarNull()
    {
        Assert.Null(await _service.AprovarAsync(99));
    }
}
