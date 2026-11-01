using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Api.DTO.Veiculo;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Tests.Controllers
{
    public class VeiculoControllerTests
    {
        private readonly Mock<IVeiculoService> _serviceMock = new();
        private readonly Mock<IUrlHelper> _urlMock = ControllerTestHelpers.CriarUrlHelperMock();
        private readonly VeiculoController _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public VeiculoControllerTests()
        {
            _sut = new VeiculoController(_serviceMock.Object) { Url = _urlMock.Object };
        }

        #region Helpers

        private static VeiculoDTO CriarDto(int id = 1, string modelo = "Gol") => new()
        {
            IdVeiculo = id,
            Placa = "ABC1D23",
            Modelo = modelo,
            Marca = "Volkswagen",
            Ano = 2020,
            IdCliente = 5,
            IdTipoVeiculo = 1
        };

        private static VeiculoRequest CriarRequest() => new()
        {
            Placa = "ABC1D23",
            Modelo = "Gol",
            Marca = "Volkswagen",
            Ano = 2020,
            IdCliente = 5,
            IdTipoVeiculo = 1
        };

        private static void AssertMapeado(VeiculoDTO dto, VeiculoResponse response)
        {
            Assert.Equal(dto.IdVeiculo, response.IdVeiculo);
            Assert.Equal(dto.Placa, response.Placa);
            Assert.Equal(dto.Modelo, response.Modelo);
            Assert.Equal(dto.Marca, response.Marca);
            Assert.Equal(dto.Ano, response.Ano);
            Assert.Equal(dto.IdCliente, response.IdCliente);
            Assert.Equal(dto.IdTipoVeiculo, response.IdTipoVeiculo);
        }

        #endregion

        #region ObterTodosAsync

        [Fact]
        public async Task ObterTodosAsync_ComVeiculos_DeveRetornarTodosMapeados()
        {
            var dtos = new List<VeiculoDTO> { CriarDto(1, "Gol"), CriarDto(2, "Uno") };
            _serviceMock.Setup(s => s.ObterTodosAsync(_ct)).ReturnsAsync(dtos);

            var result = await _sut.ObterTodosAsync(_ct);

            var itens = result.Value!.ToList();
            Assert.Equal(2, itens.Count);
            AssertMapeado(dtos[0], itens[0]);
            AssertMapeado(dtos[1], itens[1]);
        }

        [Fact]
        public async Task ObterTodosAsync_SemVeiculos_DeveRetornarOkVazio()
        {
            _serviceMock.Setup(s => s.ObterTodosAsync(_ct)).ReturnsAsync(new List<VeiculoDTO>());

            var result = await _sut.ObterTodosAsync(_ct);

            Assert.Empty(result.Value!);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_Existente_DeveRetornarOk()
        {
            var dto = CriarDto();
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterPorIdAsync(1, _ct);

            var ok = Assert.IsType<Ok<VeiculoResponse>>(result.Result);
            AssertMapeado(dto, ok.Value!);
        }

        [Fact]
        public async Task ObterPorIdAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((VeiculoDTO?)null);

            var result = await _sut.ObterPorIdAsync(99, _ct);

            Assert.IsType<NotFound<string>>(TypedResults.NotFound("Não encontrado"));
        }

        #endregion

        #region ObterPorPlacaAsync

        [Fact]
        public async Task ObterPorPlacaAsync_Existente_DeveRetornarOk()
        {
            var dto = CriarDto();
            _serviceMock.Setup(s => s.ObterPorPlacaAsync("ABC1D23", _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterPorPlacaAsync("ABC1D23", _ct);

            var ok = Assert.IsType<Ok<VeiculoResponse>>(result.Result);
            AssertMapeado(dto, ok.Value!);
        }

        [Fact]
        public async Task ObterPorPlacaAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterPorPlacaAsync("ZZZ0Z00", _ct)).ReturnsAsync((VeiculoDTO?)null);

            var result = await _sut.ObterPorPlacaAsync("ZZZ0Z00", _ct);

            Assert.IsType<NotFound<string>>(TypedResults.NotFound("Não encontrado"));
        }

        #endregion

        #region AdicionarAsync

        [Fact]
        public async Task AdicionarAsync_DeveEnviarDtoSemIdERetornarCreated()
        {
            var request = CriarRequest();
            var retorno = CriarDto(7);
            _serviceMock.Setup(s => s.AdicionarAsync(It.IsAny<VeiculoDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AdicionarAsync(request, _ct);

            Assert.Equal(ControllerTestHelpers.LocationPadrao, result.Location);
            AssertMapeado(retorno, result.Value!);
            _serviceMock.Verify(s => s.AdicionarAsync(
                It.Is<VeiculoDTO>(d => d.IdVeiculo == 0
                                    && d.Placa == request.Placa
                                    && d.Modelo == request.Modelo
                                    && d.Marca == request.Marca
                                    && d.Ano == request.Ano
                                    && d.IdCliente == request.IdCliente
                                    && d.IdTipoVeiculo == request.IdTipoVeiculo),
                _ct), Times.Once);
            _urlMock.Verify(u => u.Action(It.Is<UrlActionContext>(c => c.Action == nameof(VeiculoController.ObterPorIdAsync))), Times.Once);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_Existente_DeveEnviarDtoComIdDaRotaERetornarOk()
        {
            var request = CriarRequest();
            var retorno = CriarDto(3);
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<VeiculoDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AtualizarAsync(3, request, _ct);

            var ok = Assert.IsType<Ok<VeiculoResponse>>(result.Result);
            AssertMapeado(retorno, ok.Value!);
            _serviceMock.Verify(s => s.AtualizarAsync(
                It.Is<VeiculoDTO>(d => d.IdVeiculo == 3 && d.Placa == request.Placa && d.IdCliente == request.IdCliente),
                _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<VeiculoDTO>(), _ct)).ReturnsAsync((VeiculoDTO?)null);

            var result = await _sut.AtualizarAsync(99, CriarRequest(), _ct);

            Assert.IsType<NotFound<string>>(TypedResults.NotFound("Não encontrado"));
        }

        #endregion

        #region RemoverAsync

        [Fact]
        public async Task RemoverAsync_Existente_DeveRemoverERetornarNoContent()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(CriarDto());

            var result = await _sut.RemoverAsync(1, _ct);

            Assert.IsType<NoContent>(result.Result);
            _serviceMock.Verify(s => s.RemoverAsync(1, _ct), Times.Once);
        }

        [Fact]
        public async Task RemoverAsync_Inexistente_DeveRetornarNotFoundENaoRemover()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((VeiculoDTO?)null);

            var result = await _sut.RemoverAsync(99, _ct);

            Assert.IsType<NotFound<string>>(TypedResults.NotFound("Não encontrado"));
            _serviceMock.Verify(s => s.RemoverAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}