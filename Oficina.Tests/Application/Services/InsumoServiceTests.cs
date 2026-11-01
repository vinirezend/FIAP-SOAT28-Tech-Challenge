using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Exceptions;
using Oficina.Application.Implementations.Services;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces.Repositories;

namespace Oficina.Application.Tests.Services
{
    public class InsumoServiceTests
    {
        private const string MensagemQuantidadeInvalida = "A quantidade deve ser maior que zero.";
        private const string MensagemInsumoNaoLocalizado = "Insumo não localizado.";

        private readonly Mock<IInsumoRepository> _repositoryMock = new();
        private readonly InsumoService _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public InsumoServiceTests()
        {
            _sut = new InsumoService(_repositoryMock.Object);
        }

        #region Helpers

        private static InsumoDTO CriarDto(int id = 0) => new()
        {
            IdInsumo = id,
            Nome = "Óleo 5W30",
            Descricao = "Óleo sintético 1L",
            ValorUnitario = 45.90m,
            Ativo = true,
            QuantidadeEstoque = 10,
            QuantidadeReservada = 0
        };

        private static Insumo CriarEntidade(string nome = "Óleo 5W30") =>
            Insumo.Criar(nome, "Óleo sintético 1L", 45.90m, 10);

        private void SetupObterPorId(int id, Insumo? insumo) =>
            _repositoryMock.Setup(r => r.ObterPorIdAsync(id, _ct)).ReturnsAsync(insumo);

        #endregion

        #region AdicionarAsync

        [Fact]
        public async Task AdicionarAsync_DadosValidos_DevePersistirERetornarDto()
        {
            var dto = CriarDto();
            _repositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Insumo>(), _ct))
                .ReturnsAsync((Insumo i, CancellationToken _) => i);

            var result = await _sut.AdicionarAsync(dto, _ct);

            Assert.Equal(dto.Nome, result.Nome);
            Assert.Equal(dto.Descricao, result.Descricao);
            Assert.Equal(dto.ValorUnitario, result.ValorUnitario);
            Assert.Equal(dto.QuantidadeEstoque, result.QuantidadeEstoque);

            _repositoryMock.Verify(r => r.AdicionarAsync(
                It.Is<Insumo>(i => i.Nome == dto.Nome
                                && i.Descricao == dto.Descricao
                                && i.ValorUnitario == dto.ValorUnitario
                                && i.QuantidadeEstoque == dto.QuantidadeEstoque),
                _ct), Times.Once);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_InsumoExistente_DeveAtualizarPersistirERetornarDto()
        {
            var entidade = CriarEntidade("Nome Antigo");
            var dto = CriarDto(id: 1);
            dto.Nome = "Nome Novo";
            dto.Descricao = "Descrição nova";
            dto.ValorUnitario = 60m;
            dto.Ativo = false;
            dto.QuantidadeEstoque = 20;
            dto.QuantidadeReservada = 5;
            SetupObterPorId(1, entidade);

            var result = await _sut.AtualizarAsync(dto, _ct);

            Assert.NotNull(result);
            Assert.Equal("Nome Novo", result!.Nome);
            Assert.Equal("Descrição nova", result.Descricao);
            Assert.Equal(60m, result.ValorUnitario);
            Assert.False(result.Ativo);
            Assert.Equal(20, result.QuantidadeEstoque);
            Assert.Equal(5, result.QuantidadeReservada);
            _repositoryMock.Verify(r => r.AtualizarAsync(entidade, _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_InsumoInexistente_DeveRetornarNullENaoAtualizar()
        {
            SetupObterPorId(99, null);

            var result = await _sut.AtualizarAsync(CriarDto(id: 99), _ct);

            Assert.Null(result);
            _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Insumo>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_InsumoExistente_DeveRetornarDto()
        {
            var entidade = CriarEntidade();
            SetupObterPorId(1, entidade);

            var result = await _sut.ObterPorIdAsync(1, _ct);

            Assert.NotNull(result);
            Assert.Equal(entidade.IdInsumo, result!.IdInsumo);
            Assert.Equal(entidade.Nome, result.Nome);
            Assert.Equal(entidade.Descricao, result.Descricao);
            Assert.Equal(entidade.ValorUnitario, result.ValorUnitario);
            Assert.Equal(entidade.Ativo, result.Ativo);
            Assert.Equal(entidade.QuantidadeEstoque, result.QuantidadeEstoque);
            Assert.Equal(entidade.QuantidadeReservada, result.QuantidadeReservada);
        }

        [Fact]
        public async Task ObterPorIdAsync_InsumoInexistente_DeveRetornarNull()
        {
            SetupObterPorId(99, null);

            var result = await _sut.ObterPorIdAsync(99, _ct);

            Assert.Null(result);
        }

        #endregion

        #region ObterTodosAsync

        [Fact]
        public async Task ObterTodosAsync_ComInsumos_DeveRetornarTodosMapeados()
        {
            var entidades = new List<Insumo> { CriarEntidade("Insumo 1"), CriarEntidade("Insumo 2") };
            _repositoryMock.Setup(r => r.ObterTodosAsync(_ct)).ReturnsAsync(entidades);

            var result = await _sut.ObterTodosAsync(_ct);

            Assert.Equal(2, result.Count);
            Assert.Equal(entidades.Select(e => e.Nome), result.Select(r => r.Nome));
        }

        [Fact]
        public async Task ObterTodosAsync_SemInsumos_DeveRetornarListaVazia()
        {
            _repositoryMock.Setup(r => r.ObterTodosAsync(_ct)).ReturnsAsync(new List<Insumo>());

            var result = await _sut.ObterTodosAsync(_ct);

            Assert.Empty(result);
        }

        #endregion

        #region PesquisarAsync

        [Fact]
        public async Task PesquisarAsync_ComResultados_DeveRetornarDtosMapeados()
        {
            var entidades = new List<Insumo> { CriarEntidade("Óleo A"), CriarEntidade("Óleo B") };
            _repositoryMock.Setup(r => r.PesquisarAsync("óleo", _ct)).ReturnsAsync(entidades);

            var result = await _sut.PesquisarAsync("óleo", _ct);

            Assert.Equal(2, result.Count);
            Assert.Equal(entidades.Select(e => e.Nome), result.Select(r => r.Nome));
        }

        [Fact]
        public async Task PesquisarAsync_RepositorioRetornaNull_DeveRetornarListaVazia()
        {
            _repositoryMock.Setup(r => r.PesquisarAsync("x", _ct)).ReturnsAsync((IReadOnlyList<Insumo>?)null!);

            var result = await _sut.PesquisarAsync("x", _ct);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task PesquisarAsync_SemResultados_DeveRetornarListaVazia()
        {
            _repositoryMock.Setup(r => r.PesquisarAsync("x", _ct)).ReturnsAsync(new List<Insumo>());

            var result = await _sut.PesquisarAsync("x", _ct);

            Assert.Empty(result);
        }

        #endregion

        #region RemoverAsync

        [Fact]
        public async Task RemoverAsync_DeveDelegarParaRepositorio()
        {
            await _sut.RemoverAsync(5, _ct);

            _repositoryMock.Verify(r => r.RemoverAsync(5, _ct), Times.Once);
        }

        #endregion

        #region Consultas de quantidade

        [Fact]
        public async Task ObterQuantidadeEstoqueAsync_DeveRetornarValorDoRepositorio()
        {
            _repositoryMock.Setup(r => r.ObterQuantidadeEstoqueAsync(1, _ct)).ReturnsAsync(15);

            var result = await _sut.ObterQuantidadeEstoqueAsync(1, _ct);

            Assert.Equal(15, result);
        }

        [Fact]
        public async Task ObterQuantidadeDisponivelAsync_DeveRetornarValorDoRepositorio()
        {
            _repositoryMock.Setup(r => r.ObterQuantidadeDisponivelAsync(1, _ct)).ReturnsAsync(8);

            var result = await _sut.ObterQuantidadeDisponivelAsync(1, _ct);

            Assert.Equal(8, result);
        }

        [Fact]
        public async Task ObterQuantidadeReservadaAsync_DeveRetornarValorDoRepositorio()
        {
            _repositoryMock.Setup(r => r.ObterQuantidadeReservadaAsync(1, _ct)).ReturnsAsync(3);

            var result = await _sut.ObterQuantidadeReservadaAsync(1, _ct);

            Assert.Equal(3, result);
        }

        #endregion

        #region AdicionarEstoqueAsync

        [Fact]
        public async Task AdicionarEstoqueAsync_QuantidadeValida_DeveAdicionarERetornarQuantidades()
        {
            var entidade = CriarEntidade();
            SetupObterPorId(1, entidade);

            var result = await _sut.AdicionarEstoqueAsync(1, 5, _ct);

            _repositoryMock.Verify(r => r.AdicionarEstoqueAsync(1, 5, _ct), Times.Once);
            Assert.Equal(entidade.Nome, result.Nome);
            Assert.Equal(entidade.QuantidadeEstoque, result.QuantidadeEstoque);
            Assert.Equal(entidade.QuantidadeReservada, result.QuantidadeReservada);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task AdicionarEstoqueAsync_QuantidadeInvalida_DeveLancarBusinessException(int quantidade)
        {
            var ex = await Assert.ThrowsAsync<BusinessException>(() => _sut.AdicionarEstoqueAsync(1, quantidade, _ct));

            Assert.Equal(MensagemQuantidadeInvalida, ex.Message);
            _repositoryMock.Verify(r => r.AdicionarEstoqueAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AdicionarEstoqueAsync_InsumoNaoLocalizado_DeveLancarBusinessException()
        {
            SetupObterPorId(99, null);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _sut.AdicionarEstoqueAsync(99, 5, _ct));

            Assert.Equal(MensagemInsumoNaoLocalizado, ex.Message);
        }

        #endregion

        #region RetirarEstoqueAsync

        [Fact]
        public async Task RetirarEstoqueAsync_QuantidadeValida_DeveRetirarERetornarQuantidades()
        {
            var entidade = CriarEntidade();
            SetupObterPorId(1, entidade);

            var result = await _sut.RetirarEstoqueAsync(1, 3, _ct);

            _repositoryMock.Verify(r => r.RetirarEstoqueAsync(1, 3, _ct), Times.Once);
            Assert.Equal(entidade.Nome, result.Nome);
            Assert.Equal(entidade.QuantidadeEstoque, result.QuantidadeEstoque);
            Assert.Equal(entidade.QuantidadeReservada, result.QuantidadeReservada);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task RetirarEstoqueAsync_QuantidadeInvalida_DeveLancarBusinessException(int quantidade)
        {
            var ex = await Assert.ThrowsAsync<BusinessException>(() => _sut.RetirarEstoqueAsync(1, quantidade, _ct));

            Assert.Equal(MensagemQuantidadeInvalida, ex.Message);
            _repositoryMock.Verify(r => r.RetirarEstoqueAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RetirarEstoqueAsync_InsumoNaoLocalizado_DeveLancarBusinessException()
        {
            SetupObterPorId(99, null);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _sut.RetirarEstoqueAsync(99, 3, _ct));

            Assert.Equal(MensagemInsumoNaoLocalizado, ex.Message);
        }

        #endregion

        #region AdicionarReservaAsync

        [Fact]
        public async Task AdicionarReservaAsync_QuantidadeValida_DeveReservarERetornarQuantidades()
        {
            var entidade = CriarEntidade();
            SetupObterPorId(1, entidade);

            var result = await _sut.AdicionarReservaAsync(1, 2, _ct);

            _repositoryMock.Verify(r => r.AdicionarReservaAsync(1, 2, _ct), Times.Once);
            Assert.Equal(entidade.Nome, result.Nome);
            Assert.Equal(entidade.QuantidadeEstoque, result.QuantidadeEstoque);
            Assert.Equal(entidade.QuantidadeReservada, result.QuantidadeReservada);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task AdicionarReservaAsync_QuantidadeInvalida_DeveLancarBusinessException(int quantidade)
        {
            var ex = await Assert.ThrowsAsync<BusinessException>(() => _sut.AdicionarReservaAsync(1, quantidade, _ct));

            Assert.Equal(MensagemQuantidadeInvalida, ex.Message);
            _repositoryMock.Verify(r => r.AdicionarReservaAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AdicionarReservaAsync_InsumoNaoLocalizado_DeveLancarBusinessException()
        {
            SetupObterPorId(99, null);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _sut.AdicionarReservaAsync(99, 2, _ct));

            Assert.Equal(MensagemInsumoNaoLocalizado, ex.Message);
        }

        #endregion

        #region RetirarReservaAsync

        [Fact]
        public async Task RetirarReservaAsync_QuantidadeValida_DeveRetirarReservaERetornarQuantidades()
        {
            var entidade = CriarEntidade();
            SetupObterPorId(1, entidade);

            var result = await _sut.RetirarReservaAsync(1, 1, _ct);

            _repositoryMock.Verify(r => r.RetirarReservaAsync(1, 1, _ct), Times.Once);
            Assert.Equal(entidade.Nome, result.Nome);
            Assert.Equal(entidade.QuantidadeEstoque, result.QuantidadeEstoque);
            Assert.Equal(entidade.QuantidadeReservada, result.QuantidadeReservada);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task RetirarReservaAsync_QuantidadeInvalida_DeveLancarBusinessException(int quantidade)
        {
            var ex = await Assert.ThrowsAsync<BusinessException>(() => _sut.RetirarReservaAsync(1, quantidade, _ct));

            Assert.Equal(MensagemQuantidadeInvalida, ex.Message);
            _repositoryMock.Verify(r => r.RetirarReservaAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RetirarReservaAsync_InsumoNaoLocalizado_DeveLancarBusinessException()
        {
            SetupObterPorId(99, null);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _sut.RetirarReservaAsync(99, 1, _ct));

            Assert.Equal(MensagemInsumoNaoLocalizado, ex.Message);
        }

        #endregion
    }
}