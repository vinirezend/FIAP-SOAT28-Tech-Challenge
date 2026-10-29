using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Exceptions;
using Oficina.Application.Implementations.Services;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Tests.Application.Services;

public class OrdemServicoServiceTests
{
    private const int IdColaborador = 2;
    private static readonly DateTimeOffset Agora = new(2026, 10, 27, 15, 30, 0, TimeSpan.Zero);

    private readonly Mock<IOrdemServicoRepository> _repositoryMock = new();
    private readonly Mock<IServicoRepository> _servicoRepositoryMock = new();
    private readonly Mock<IInsumoRepository> _insumoRepositoryMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<TimeProvider> _timeProviderMock = new();
    private readonly OrdemServicoService _service;

    public OrdemServicoServiceTests()
    {
        _currentUserMock.Setup(u => u.IdColaborador).Returns(IdColaborador);
        _timeProviderMock.Setup(t => t.GetUtcNow()).Returns(Agora);

        _service = new OrdemServicoService(
            _repositoryMock.Object,
            _servicoRepositoryMock.Object,
            _insumoRepositoryMock.Object,
            _currentUserMock.Object,
            _timeProviderMock.Object);
    }

    private void ConfigurarOs(StatusOs? status) =>
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(OrdemServico.Reconstituir(1, 1, null, null, 0, status));

    private static Servico CriarServico(int idServico, decimal valorEstipulado, bool ativo = true) =>
        Servico.Reconstituir(idServico, "Troca de óleo", "Descrição", valorEstipulado, ativo);

    private static Insumo CriarInsumo(int idInsumo, decimal valorUnitario, bool ativo = true, int quantidadeEstoque = 100, int quantidadeReservada = 0) =>
        Insumo.Reconstituir(idInsumo, "Óleo", "Descrição", valorUnitario, ativo, quantidadeEstoque, quantidadeReservada);

    [Fact]
    public async Task AdicionarAsync_DeveCriarOsRecebidaEmNomeDoColaboradorLogado()
    {
        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<OrdemServico>(), IdColaborador, Agora.DateTime))
            .ReturnsAsync((OrdemServico os, int _, DateTime _) => OrdemServico.Reconstituir(10, os.IdVeiculo, null, null, 0, os.Status));

        var resultado = await _service.AdicionarAsync(new OrdemServicoDTO { IdVeiculo = 3 });

        Assert.Equal(10, resultado.IdOs);
        Assert.Equal(3, resultado.IdVeiculo);
        Assert.Equal((int)StatusOs.Recebida, resultado.IdStatus);
        Assert.Equal(nameof(StatusOs.Recebida), resultado.Status);
    }

    [Fact]
    public async Task AdicionarAsync_UsuarioSemColaborador_DeveLancarExcecao()
    {
        _currentUserMock.Setup(u => u.IdColaborador).Returns((int?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdicionarAsync(new OrdemServicoDTO { IdVeiculo = 3 }));
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<OrdemServico>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task AlterarStatusAsync_ParaEmExecucao_DeveGravarStatusEDataInicio()
    {
        ConfigurarOs(StatusOs.AguardandoAprovacao);

        var resultado = await _service.AlterarStatusAsync(1, (int)StatusOs.EmExecucao);

        Assert.NotNull(resultado);
        Assert.Equal((int)StatusOs.EmExecucao, resultado.IdStatus);
        Assert.Equal(Agora.DateTime, resultado.DataInicio);
        _repositoryMock.Verify(r => r.AlterarStatusAsync(
            It.Is<OrdemServico>(os => os.Status == StatusOs.EmExecucao && os.DataInicio == Agora.DateTime),
            IdColaborador,
            Agora.DateTime), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusAsync_TransicaoInvalida_NaoDeveGravar()
    {
        ConfigurarOs(StatusOs.Recebida);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AlterarStatusAsync(1, (int)StatusOs.Finalizada));
        _repositoryMock.Verify(r => r.AlterarStatusAsync(It.IsAny<OrdemServico>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task AlterarStatusAsync_OsInexistente_DeveRetornarNull()
    {
        var resultado = await _service.AlterarStatusAsync(99, (int)StatusOs.EmDiagnostico);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAlterarVeiculo()
    {
        ConfigurarOs(StatusOs.Recebida);

        var resultado = await _service.AtualizarAsync(new OrdemServicoDTO { IdOs = 1, IdVeiculo = 5 });

        Assert.NotNull(resultado);
        Assert.Equal(5, resultado.IdVeiculo);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.Is<OrdemServico>(os => os.IdVeiculo == 5)), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_OsInexistente_DeveRetornarNull()
    {
        var resultado = await _service.AtualizarAsync(new OrdemServicoDTO { IdOs = 99, IdVeiculo = 5 });

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterHistoricoStatusAsync_DeveRetornarHistorico()
    {
        ConfigurarOs(StatusOs.EmDiagnostico);
        _repositoryMock
            .Setup(r => r.ObterHistoricoStatusAsync(1))
            .ReturnsAsync(
            [
                OrdemServicoStatus.Reconstituir(1, 1, StatusOs.Recebida, 2, Agora.DateTime),
                OrdemServicoStatus.Reconstituir(2, 1, StatusOs.EmDiagnostico, 3, Agora.DateTime)
            ]);

        var resultado = await _service.ObterHistoricoStatusAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal([nameof(StatusOs.Recebida), nameof(StatusOs.EmDiagnostico)], resultado.Select(h => h.Status));
    }

    [Fact]
    public async Task ObterHistoricoStatusAsync_OsInexistente_DeveRetornarNull()
    {
        var resultado = await _service.ObterHistoricoStatusAsync(99);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarOrdensServico()
    {
        _repositoryMock
            .Setup(r => r.ObterTodosAsync())
            .ReturnsAsync([OrdemServico.Reconstituir(1, 1, null, null, 0, StatusOs.Recebida)]);

        var resultado = await _service.ObterTodosAsync();

        Assert.Single(resultado);
    }

    [Fact]
    public async Task ObterPorVeiculoAsync_DeveRetornarOrdensServicoDoVeiculo()
    {
        _repositoryMock
            .Setup(r => r.ObterPorVeiculoAsync(1))
            .ReturnsAsync([OrdemServico.Reconstituir(1, 1, null, null, 0, StatusOs.Recebida)]);

        var resultado = await _service.ObterPorVeiculoAsync(1);

        Assert.Equal(1, Assert.Single(resultado).IdVeiculo);
    }

    [Fact]
    public async Task ObterPorIdAsync_OsInexistente_DeveRetornarNull()
    {
        var resultado = await _service.ObterPorIdAsync(99);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task RemoverAsync_OsRecebida_DeveRemover()
    {
        ConfigurarOs(StatusOs.Recebida);
        _repositoryMock.Setup(r => r.ObterInsumosAsync(1)).ReturnsAsync([]);

        await _service.RemoverAsync(1);

        _repositoryMock.Verify(r => r.RemoverAsync(1), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_OsEmAndamento_NaoDeveRemover()
    {
        ConfigurarOs(StatusOs.EmExecucao);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RemoverAsync(1));
        _repositoryMock.Verify(r => r.RemoverAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RemoverAsync_ComInsumosVinculados_DeveLiberarReservaAntesDeRemover()
    {
        ConfigurarOs(StatusOs.Recebida);
        var insumoItem = OrdemServicoInsumo.Reconstituir(11, 1, 7, 3, 50m);
        _repositoryMock.Setup(r => r.ObterInsumosAsync(1)).ReturnsAsync([insumoItem]);

        await _service.RemoverAsync(1);

        _insumoRepositoryMock.Verify(r => r.RetirarReservaAsync(7, 3), Times.Once);
        _repositoryMock.Verify(r => r.RemoverAsync(1), Times.Once);
    }

    [Fact]
    public async Task ObterServicosAsync_OsInexistente_DeveRetornarNull()
    {
        var resultado = await _service.ObterServicosAsync(99);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterServicosAsync_DeveListarItensComValorTotal()
    {
        ConfigurarOs(StatusOs.Recebida);
        _repositoryMock.Setup(r => r.ObterServicosAsync(1)).ReturnsAsync([OrdemServicoServico.Reconstituir(10, 1, 5, 2, 100m)]);

        var resultado = await _service.ObterServicosAsync(1);

        Assert.Equal(200m, Assert.Single(resultado!).ValorTotal);
    }

    [Fact]
    public async Task AdicionarServicoAsync_DeveIncluirItemComPrecoAtualDoCatalogo()
    {
        ConfigurarOs(StatusOs.Recebida);
        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(CriarServico(5, 100m));
        _repositoryMock
            .Setup(r => r.AdicionarServicoAsync(It.IsAny<OrdemServicoServico>()))
            .ReturnsAsync((OrdemServicoServico item) => OrdemServicoServico.Reconstituir(10, item.IdOs, item.IdServico, item.Quantidade, item.ValorCobrado));

        var resultado = await _service.AdicionarServicoAsync(1, 5, 2);

        Assert.NotNull(resultado);
        Assert.Equal(100m, resultado.ValorCobrado);
        Assert.Equal(200m, resultado.ValorTotal);
    }

    [Fact]
    public async Task AdicionarServicoAsync_OsInexistente_DeveRetornarNull()
    {
        var resultado = await _service.AdicionarServicoAsync(99, 5, 2);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AdicionarServicoAsync_ForaDeRecebidaOuDiagnostico_DeveLancarExcecao()
    {
        ConfigurarOs(StatusOs.AguardandoAprovacao);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdicionarServicoAsync(1, 5, 2));
        _repositoryMock.Verify(r => r.AdicionarServicoAsync(It.IsAny<OrdemServicoServico>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarServicoAsync_ServicoInativo_DeveLancarExcecao()
    {
        ConfigurarOs(StatusOs.Recebida);
        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(CriarServico(5, 100m, ativo: false));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdicionarServicoAsync(1, 5, 2));
        _repositoryMock.Verify(r => r.AdicionarServicoAsync(It.IsAny<OrdemServicoServico>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarServicoAsync_ServicoInexistente_DeveLancarExcecao()
    {
        ConfigurarOs(StatusOs.Recebida);
        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync((Servico?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdicionarServicoAsync(1, 5, 2));
    }

    [Fact]
    public async Task RemoverServicoAsync_DeveRemoverItemEncontrado()
    {
        ConfigurarOs(StatusOs.EmDiagnostico);
        var item = OrdemServicoServico.Reconstituir(10, 1, 5, 2, 100m);
        _repositoryMock.Setup(r => r.ObterServicosAsync(1)).ReturnsAsync([item]);

        await _service.RemoverServicoAsync(1, 10);

        _repositoryMock.Verify(r => r.RemoverServicoAsync(item), Times.Once);
    }

    [Fact]
    public async Task RemoverServicoAsync_ItemInexistente_NaoDeveChamarRepositorio()
    {
        ConfigurarOs(StatusOs.EmDiagnostico);
        _repositoryMock.Setup(r => r.ObterServicosAsync(1)).ReturnsAsync([]);

        await _service.RemoverServicoAsync(1, 999);

        _repositoryMock.Verify(r => r.RemoverServicoAsync(It.IsAny<OrdemServicoServico>()), Times.Never);
    }

    [Fact]
    public async Task RemoverServicoAsync_ForaDeRecebidaOuDiagnostico_DeveLancarExcecao()
    {
        ConfigurarOs(StatusOs.Finalizada);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RemoverServicoAsync(1, 10));
    }

    [Fact]
    public async Task AlterarQuantidadeServicoAsync_DeveAtualizarQuantidadeEPreco()
    {
        ConfigurarOs(StatusOs.Recebida);
        var item = OrdemServicoServico.Reconstituir(10, 1, 5, 2, 100m);
        _repositoryMock.Setup(r => r.ObterServicosAsync(1)).ReturnsAsync([item]);
        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(CriarServico(5, 120m));

        var resultado = await _service.AlterarQuantidadeServicoAsync(1, 10, 4);

        Assert.NotNull(resultado);
        Assert.Equal(4, resultado.Quantidade);
        Assert.Equal(120m, resultado.ValorCobrado);
        Assert.Equal(480m, resultado.ValorTotal);
        _repositoryMock.Verify(r => r.AtualizarQuantidadeServicoAsync(item), Times.Once);
    }

    [Fact]
    public async Task ObterInsumosAsync_OsInexistente_DeveRetornarNull()
    {
        var resultado = await _service.ObterInsumosAsync(99);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AdicionarInsumoAsync_DeveReservarEstoqueEIncluirItem()
    {
        ConfigurarOs(StatusOs.Recebida);
        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(7)).ReturnsAsync(CriarInsumo(7, 50m));
        _repositoryMock
            .Setup(r => r.AdicionarInsumoAsync(It.IsAny<OrdemServicoInsumo>()))
            .ReturnsAsync((OrdemServicoInsumo item) => OrdemServicoInsumo.Reconstituir(11, item.IdOs, item.IdInsumo, item.Quantidade, item.ValorCobrado));

        var resultado = await _service.AdicionarInsumoAsync(1, 7, 3);

        Assert.NotNull(resultado);
        Assert.Equal(150m, resultado.ValorTotal);
        _insumoRepositoryMock.Verify(r => r.AdicionarReservaAsync(7, 3), Times.Once);
    }

    [Fact]
    public async Task AdicionarInsumoAsync_InsumoInativo_DeveLancarExcecaoSemReservar()
    {
        ConfigurarOs(StatusOs.Recebida);
        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(7)).ReturnsAsync(CriarInsumo(7, 50m, ativo: false));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdicionarInsumoAsync(1, 7, 3));
        _insumoRepositoryMock.Verify(r => r.AdicionarReservaAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarInsumoAsync_EstoqueInsuficiente_DevePropagarExcecaoSemIncluirItem()
    {
        ConfigurarOs(StatusOs.Recebida);
        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(7)).ReturnsAsync(CriarInsumo(7, 50m, quantidadeEstoque: 2));
        _insumoRepositoryMock
            .Setup(r => r.AdicionarReservaAsync(7, 3))
            .ThrowsAsync(new BusinessException("Insumo não localizado ou quantidade disponível insuficiente para reserva."));

        await Assert.ThrowsAsync<BusinessException>(() => _service.AdicionarInsumoAsync(1, 7, 3));
        _repositoryMock.Verify(r => r.AdicionarInsumoAsync(It.IsAny<OrdemServicoInsumo>()), Times.Never);
    }

    [Fact]
    public async Task RemoverInsumoAsync_DeveLiberarReservaERemoverItem()
    {
        ConfigurarOs(StatusOs.EmDiagnostico);
        var item = OrdemServicoInsumo.Reconstituir(11, 1, 7, 3, 50m);
        _repositoryMock.Setup(r => r.ObterInsumosAsync(1)).ReturnsAsync([item]);

        await _service.RemoverInsumoAsync(1, 11);

        _insumoRepositoryMock.Verify(r => r.RetirarReservaAsync(7, 3), Times.Once);
        _repositoryMock.Verify(r => r.RemoverInsumoAsync(item), Times.Once);
    }

    [Fact]
    public async Task AlterarQuantidadeInsumoAsync_AumentoDeQuantidade_DeveReservarDiferenca()
    {
        ConfigurarOs(StatusOs.Recebida);
        var item = OrdemServicoInsumo.Reconstituir(11, 1, 7, 2, 50m);
        _repositoryMock.Setup(r => r.ObterInsumosAsync(1)).ReturnsAsync([item]);
        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(7)).ReturnsAsync(CriarInsumo(7, 60m));

        var resultado = await _service.AlterarQuantidadeInsumoAsync(1, 11, 5);

        Assert.NotNull(resultado);
        Assert.Equal(5, resultado.Quantidade);
        Assert.Equal(60m, resultado.ValorCobrado);
        _insumoRepositoryMock.Verify(r => r.AdicionarReservaAsync(7, 3), Times.Once);
        _insumoRepositoryMock.Verify(r => r.RetirarReservaAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AlterarQuantidadeInsumoAsync_ReducaoDeQuantidade_DeveLiberarDiferenca()
    {
        ConfigurarOs(StatusOs.Recebida);
        var item = OrdemServicoInsumo.Reconstituir(11, 1, 7, 5, 50m);
        _repositoryMock.Setup(r => r.ObterInsumosAsync(1)).ReturnsAsync([item]);
        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(7)).ReturnsAsync(CriarInsumo(7, 60m));

        await _service.AlterarQuantidadeInsumoAsync(1, 11, 2);

        _insumoRepositoryMock.Verify(r => r.RetirarReservaAsync(7, 3), Times.Once);
        _insumoRepositoryMock.Verify(r => r.AdicionarReservaAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AlterarStatusAsync_ParaAguardandoAprovacao_DeveCongelarValorComPrecoAtualDoCatalogo()
    {
        ConfigurarOs(StatusOs.EmDiagnostico);
        var servicoItem = OrdemServicoServico.Reconstituir(10, 1, 5, 2, 90m);
        var insumoItem = OrdemServicoInsumo.Reconstituir(11, 1, 7, 3, 40m);
        _repositoryMock.Setup(r => r.ObterServicosAsync(1)).ReturnsAsync([servicoItem]);
        _repositoryMock.Setup(r => r.ObterInsumosAsync(1)).ReturnsAsync([insumoItem]);
        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(CriarServico(5, 100m));
        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(7)).ReturnsAsync(CriarInsumo(7, 50m));

        var resultado = await _service.AlterarStatusAsync(1, (int)StatusOs.AguardandoAprovacao);

        Assert.NotNull(resultado);
        Assert.Equal(350m, resultado.Valor);
        _repositoryMock.Verify(r => r.AtualizarQuantidadeServicoAsync(It.Is<OrdemServicoServico>(i => i.ValorCobrado == 100m)), Times.Once);
        _repositoryMock.Verify(r => r.AtualizarQuantidadeInsumoAsync(It.Is<OrdemServicoInsumo>(i => i.ValorCobrado == 50m)), Times.Once);
        _insumoRepositoryMock.Verify(r => r.AdicionarReservaAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _insumoRepositoryMock.Verify(r => r.RetirarReservaAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
