using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Implementations.Services;
using Oficina.Domain.Entities;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositories;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.Tests.Services
{
    public class VeiculoServiceTests
    {
        // Valores válidos (ajuste se a validação da Placa / do enum for diferente)
        private const string Placa1 = "ABC1D23";
        private const string Placa2 = "XYZ9K88";
        private const int TipoVeiculoValido = 1;

        private readonly Mock<IVeiculoRepository> _repositoryMock = new();
        private readonly VeiculoService _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public VeiculoServiceTests()
        {
            _sut = new VeiculoService(_repositoryMock.Object);
        }

        #region Helpers

        private static VeiculoDTO CriarDto(int id = 0, string placa = Placa1) => new()
        {
            IdVeiculo = id,
            IdCliente = 1,
            Placa = placa,
            Modelo = "Gol",
            Marca = "Volkswagen",
            Ano = 2020,
            IdTipoVeiculo = TipoVeiculoValido
        };

        private static Veiculo CriarEntidade(string modelo = "Gol") =>
            Veiculo.Criar(
                1,
                Placa.Criar(Placa1),
                modelo,
                "Volkswagen",
                2020,
                (TipoVeiculo)TipoVeiculoValido);

        #endregion

        #region AdicionarAsync

        [Fact]
        public async Task AdicionarAsync_DadosValidos_DevePersistirERetornarDto()
        {
            var dto = CriarDto();
            _repositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Veiculo>(), _ct))
                .ReturnsAsync((Veiculo v, CancellationToken _) => v);

            var result = await _sut.AdicionarAsync(dto, _ct);

            Assert.Equal(dto.IdCliente, result.IdCliente);
            Assert.Equal(dto.Placa, result.Placa);
            Assert.Equal(dto.Modelo, result.Modelo);
            Assert.Equal(dto.Marca, result.Marca);
            Assert.Equal(dto.Ano, result.Ano);
            Assert.Equal(dto.IdTipoVeiculo, result.IdTipoVeiculo);

            _repositoryMock.Verify(r => r.AdicionarAsync(
                It.Is<Veiculo>(v => v.IdCliente == dto.IdCliente
                                 && v.Placa.Numero == dto.Placa
                                 && v.Modelo == dto.Modelo
                                 && v.Marca == dto.Marca
                                 && v.Ano == dto.Ano
                                 && (int)v.IdTipoVeiculo == dto.IdTipoVeiculo),
                _ct), Times.Once);
        }

        [Fact]
        public async Task AdicionarAsync_PlacaInvalida_DeveLancarExcecaoENaoPersistir()
        {
            var dto = CriarDto(placa: "123");

            await Assert.ThrowsAnyAsync<Exception>(() => _sut.AdicionarAsync(dto, _ct));

            _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_VeiculoExistente_DeveAtualizarPersistirERetornarDto()
        {
            var entidade = CriarEntidade("Modelo Antigo");
            var dto = CriarDto(id: 1, placa: Placa2);
            dto.IdCliente = 2;
            dto.Modelo = "Modelo Novo";
            dto.Marca = "Marca Nova";
            dto.Ano = 2022;
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1, _ct)).ReturnsAsync(entidade);

            var result = await _sut.AtualizarAsync(dto, _ct);

            Assert.NotNull(result);
            Assert.Equal(2, result!.IdCliente);
            Assert.Equal(Placa2, result.Placa);
            Assert.Equal("Modelo Novo", result.Modelo);
            Assert.Equal("Marca Nova", result.Marca);
            Assert.Equal(2022, result.Ano);
            Assert.Equal(TipoVeiculoValido, result.IdTipoVeiculo);
            _repositoryMock.Verify(r => r.AtualizarAsync(entidade, _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_VeiculoInexistente_DeveRetornarNullENaoAtualizar()
        {
            _repositoryMock.Setup(r => r.ObterPorIdAsync(99, _ct)).ReturnsAsync((Veiculo?)null);

            var result = await _sut.AtualizarAsync(CriarDto(id: 99), _ct);

            Assert.Null(result);
            _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarAsync_PlacaInvalida_DeveLancarExcecaoENaoAtualizar()
        {
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1, _ct)).ReturnsAsync(CriarEntidade());
            var dto = CriarDto(id: 1, placa: "123");

            await Assert.ThrowsAnyAsync<Exception>(() => _sut.AtualizarAsync(dto, _ct));

            _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_VeiculoExistente_DeveRetornarDto()
        {
            var entidade = CriarEntidade();
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1, _ct)).ReturnsAsync(entidade);

            var result = await _sut.ObterPorIdAsync(1, _ct);

            Assert.NotNull(result);
            Assert.Equal(entidade.IdVeiculo, result!.IdVeiculo);
            Assert.Equal(entidade.IdCliente, result.IdCliente);
            Assert.Equal(entidade.Placa.Numero, result.Placa);
            Assert.Equal(entidade.Modelo, result.Modelo);
            Assert.Equal(entidade.Marca, result.Marca);
            Assert.Equal(entidade.Ano, result.Ano);
            Assert.Equal((int)entidade.IdTipoVeiculo, result.IdTipoVeiculo);
        }

        [Fact]
        public async Task ObterPorIdAsync_VeiculoInexistente_DeveRetornarNull()
        {
            _repositoryMock.Setup(r => r.ObterPorIdAsync(99, _ct)).ReturnsAsync((Veiculo?)null);

            var result = await _sut.ObterPorIdAsync(99, _ct);

            Assert.Null(result);
        }

        #endregion

        #region ObterPorPlacaAsync

        [Fact]
        public async Task ObterPorPlacaAsync_VeiculoExistente_DeveRetornarDto()
        {
            var entidade = CriarEntidade();
            _repositoryMock.Setup(r => r.ObterPorPlacaAsync(Placa1, _ct)).ReturnsAsync(entidade);

            var result = await _sut.ObterPorPlacaAsync(Placa1, _ct);

            Assert.NotNull(result);
            Assert.Equal(entidade.Placa.Numero, result!.Placa);
            Assert.Equal(entidade.Modelo, result.Modelo);
        }

        [Fact]
        public async Task ObterPorPlacaAsync_VeiculoInexistente_DeveRetornarNull()
        {
            _repositoryMock.Setup(r => r.ObterPorPlacaAsync("ZZZ0Z00", _ct)).ReturnsAsync((Veiculo?)null);

            var result = await _sut.ObterPorPlacaAsync("ZZZ0Z00", _ct);

            Assert.Null(result);
        }

        #endregion

        #region ObterTodosAsync

        [Fact]
        public async Task ObterTodosAsync_ComVeiculos_DeveRetornarTodosMapeados()
        {
            var entidades = new List<Veiculo> { CriarEntidade("Gol"), CriarEntidade("Uno") };
            _repositoryMock.Setup(r => r.ObterTodosAsync(_ct)).ReturnsAsync(entidades);

            var result = await _sut.ObterTodosAsync(_ct);

            Assert.Equal(2, result.Count);
            Assert.Equal(entidades.Select(e => e.Modelo), result.Select(r => r.Modelo));
        }

        [Fact]
        public async Task ObterTodosAsync_SemVeiculos_DeveRetornarListaVazia()
        {
            _repositoryMock.Setup(r => r.ObterTodosAsync(_ct)).ReturnsAsync(new List<Veiculo>());

            var result = await _sut.ObterTodosAsync(_ct);

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
    }
}