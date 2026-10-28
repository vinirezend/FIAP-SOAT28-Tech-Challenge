using Moq;
using Oficina.Application.DTOs;
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
}
