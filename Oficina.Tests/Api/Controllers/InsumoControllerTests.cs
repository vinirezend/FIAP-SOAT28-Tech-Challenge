using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Api.DTO.Insumo;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Tests.Controllers
{
    public class InsumoControllerTests
    {
        private readonly Mock<IInsumoService> _serviceMock = new();
        private readonly Mock<IUrlHelper> _urlMock = ControllerTestHelpers.CriarUrlHelperMock();
        private readonly InsumoController _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public InsumoControllerTests()
        {
            _sut = new InsumoController(_serviceMock.Object) { Url = _urlMock.Object };
        }

        #region Helpers

        private static InsumoDTO CriarDto(int id = 1, string nome = "Óleo 5W30") => new()
        {
            IdInsumo = id,
            Nome = nome,
            Descricao = "Óleo sintético 1L",
            ValorUnitario = 45.90m,
            Ativo = true,
            QuantidadeEstoque = 10,
            QuantidadeReservada = 2
        };

        private static InsumoQuantidadeDTO CriarQuantidadeDto() => new()
        {
            Nome = "Óleo 5W30",
            QuantidadeEstoque = 15,
            QuantidadeReservada = 3
        };

        private static void AssertMapeado(InsumoDTO dto, InsumoResponse response)
        {
            Assert.Equal(dto.IdInsumo, response.IdInsumo);
            Assert.Equal(dto.Nome, response.Nome);
            Assert.Equal(dto.Descricao, response.Descricao);
            Assert.Equal(dto.ValorUnitario, response.ValorUnitario);
            Assert.Equal(dto.Ativo, response.Ativo);
            Assert.Equal(dto.QuantidadeEstoque, response.QuantidadeEstoque);
            Assert.Equal(dto.QuantidadeReservada, response.QuantidadeReservada);
        }

        private static void AssertQuantidade(InsumoQuantidadeDTO dto, InsumoQuantidadeResponse response)
        {
            Assert.Equal(dto.Nome, response.Nome);
            Assert.Equal(dto.QuantidadeEstoque, response.QuantidadeEstoque);
            Assert.Equal(dto.QuantidadeReservada, response.QuantidadeReservada);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_Existente_DeveRetornarOk()
        {
            var dto = CriarDto();
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterPorIdAsync(1, _ct);

            var ok = Assert.IsType<Ok<InsumoResponse>>(result.Result);
            AssertMapeado(dto, ok.Value!);
        }

        [Fact]
        public async Task ObterPorIdAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((InsumoDTO?)null);

            var result = await _sut.ObterPorIdAsync(99, _ct);

            Assert.IsType<NotFound<string>>(TypedResults.NotFound("Não encontrado"));
        }

        #endregion

        #region ObterTodosAsync / PesquisarAsync

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarTodosMapeados()
        {
            var dtos = new List<InsumoDTO> { CriarDto(1, "Insumo 1"), CriarDto(2, "Insumo 2") };
            _serviceMock.Setup(s => s.ObterTodosAsync(_ct)).ReturnsAsync(dtos);

            var result = await _sut.ObterTodosAsync(_ct);

            var itens = result.Value!.ToList();
            Assert.Equal(2, itens.Count);
            AssertMapeado(dtos[0], itens[0]);
            AssertMapeado(dtos[1], itens[1]);
        }

        [Fact]
        public async Task PesquisarAsync_DeveRepassarTermoERetornarMapeados()
        {
            var dtos = new List<InsumoDTO> { CriarDto(1, "Óleo A") };
            _serviceMock.Setup(s => s.PesquisarAsync("óleo", _ct)).ReturnsAsync(dtos);

            var result = await _sut.PesquisarAsync("óleo", _ct);

            var itens = result.Value!.ToList();
            Assert.Single(itens);
            AssertMapeado(dtos[0], itens[0]);
            _serviceMock.Verify(s => s.PesquisarAsync("óleo", _ct), Times.Once);
        }

        #endregion

        #region AdicionarAsync

        [Fact]
        public async Task AdicionarAsync_DeveEnviarDtoAoServiceERetornarCreated()
        {
            var request = new InsumoCreateRequest
            {
                Nome = "Óleo 5W30",
                Descricao = "Óleo sintético 1L",
                ValorUnitario = 45.90m,
                QuantidadeEstoque = 10
            };
            var retorno = CriarDto(7);
            _serviceMock.Setup(s => s.AdicionarAsync(It.IsAny<InsumoDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AdicionarAsync(request, _ct);

            Assert.Equal(ControllerTestHelpers.LocationPadrao, result.Location);
            AssertMapeado(retorno, result.Value!);
            _serviceMock.Verify(s => s.AdicionarAsync(
                It.Is<InsumoDTO>(d => d.Nome == request.Nome
                                   && d.Descricao == request.Descricao
                                   && d.ValorUnitario == request.ValorUnitario
                                   && d.QuantidadeEstoque == request.QuantidadeEstoque),
                _ct), Times.Once);
            _urlMock.Verify(u => u.Action(It.Is<UrlActionContext>(c => c.Action == nameof(InsumoController.ObterPorIdAsync))), Times.Once);
        }

        [Fact]
        public async Task AdicionarAsync_DescricaoNula_DeveEnviarStringVazia()
        {
            var request = new InsumoCreateRequest { Nome = "Filtro", Descricao = null, ValorUnitario = 20m, QuantidadeEstoque = 5 };
            _serviceMock.Setup(s => s.AdicionarAsync(It.IsAny<InsumoDTO>(), _ct)).ReturnsAsync(CriarDto());

            await _sut.AdicionarAsync(request, _ct);

            _serviceMock.Verify(s => s.AdicionarAsync(It.Is<InsumoDTO>(d => d.Descricao == string.Empty), _ct), Times.Once);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_Existente_DeveEnviarDtoComIdDaRotaERetornarOk()
        {
            var request = new InsumoUpdateRequest
            {
                Nome = "Novo",
                Descricao = "Nova desc",
                ValorUnitario = 60m,
                Ativo = false,
                QuantidadeEstoque = 20,
                QuantidadeReservada = 5
            };
            var retorno = CriarDto(3, "Novo");
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<InsumoDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AtualizarAsync(3, request, _ct);

            var ok = Assert.IsType<Ok<InsumoResponse>>(result.Result);
            AssertMapeado(retorno, ok.Value!);
            _serviceMock.Verify(s => s.AtualizarAsync(
                It.Is<InsumoDTO>(d => d.IdInsumo == 3
                                   && d.Nome == request.Nome
                                   && d.Descricao == request.Descricao
                                   && d.ValorUnitario == request.ValorUnitario
                                   && d.Ativo == request.Ativo
                                   && d.QuantidadeEstoque == request.QuantidadeEstoque
                                   && d.QuantidadeReservada == request.QuantidadeReservada),
                _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_DescricaoNula_DeveEnviarStringVazia()
        {
            var request = new InsumoUpdateRequest { Nome = "Novo", Descricao = null };
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<InsumoDTO>(), _ct)).ReturnsAsync(CriarDto());

            await _sut.AtualizarAsync(1, request, _ct);

            _serviceMock.Verify(s => s.AtualizarAsync(It.Is<InsumoDTO>(d => d.Descricao == string.Empty), _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<InsumoDTO>(), _ct)).ReturnsAsync((InsumoDTO?)null);

            var result = await _sut.AtualizarAsync(99, new InsumoUpdateRequest(), _ct);

            Assert.IsType<NotFound<string>>(TypedResults.NotFound("Não encontrado"));
        }

        #endregion

        #region DeletarAsync

        [Fact]
        public async Task DeletarAsync_Existente_DeveRemoverERetornarNoContent()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(CriarDto());

            var result = await _sut.DeletarAsync(1, _ct);

            Assert.IsType<NoContent>(result.Result);
            _serviceMock.Verify(s => s.RemoverAsync(1, _ct), Times.Once);
        }

        [Fact]
        public async Task DeletarAsync_Inexistente_DeveRetornarNotFoundENaoRemover()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((InsumoDTO?)null);

            var result = await _sut.DeletarAsync(99, _ct);

            Assert.IsType<NotFound<string>>(TypedResults.NotFound("Não encontrado"));
            _serviceMock.Verify(s => s.RemoverAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Estoque e reserva

        [Fact]
        public async Task AdicionarEstoqueAsync_DeveDelegarERetornarOkMapeado()
        {
            var dto = CriarQuantidadeDto();
            _serviceMock.Setup(s => s.AdicionarEstoqueAsync(1, 5, _ct)).ReturnsAsync(dto);

            var result = await _sut.AdicionarEstoqueAsync(1, 5, _ct);

            AssertQuantidade(dto, result.Value!);
            _serviceMock.Verify(s => s.AdicionarEstoqueAsync(1, 5, _ct), Times.Once);
        }

        [Fact]
        public async Task RetirarEstoqueAsync_DeveDelegarERetornarOkMapeado()
        {
            var dto = CriarQuantidadeDto();
            _serviceMock.Setup(s => s.RetirarEstoqueAsync(1, 5, _ct)).ReturnsAsync(dto);

            var result = await _sut.RetirarEstoqueAsync(1, 5, _ct);

            AssertQuantidade(dto, result.Value!);
            _serviceMock.Verify(s => s.RetirarEstoqueAsync(1, 5, _ct), Times.Once);
        }

        [Fact]
        public async Task AdicionarReservaAsync_DeveDelegarERetornarOkMapeado()
        {
            var dto = CriarQuantidadeDto();
            _serviceMock.Setup(s => s.AdicionarReservaAsync(1, 2, _ct)).ReturnsAsync(dto);

            var result = await _sut.AdicionarReservaAsync(1, 2, _ct);

            AssertQuantidade(dto, result.Value!);
            _serviceMock.Verify(s => s.AdicionarReservaAsync(1, 2, _ct), Times.Once);
        }

        [Fact]
        public async Task RetirarReservaAsync_DeveDelegarERetornarOkMapeado()
        {
            var dto = CriarQuantidadeDto();
            _serviceMock.Setup(s => s.RetirarReservaAsync(1, 2, _ct)).ReturnsAsync(dto);

            var result = await _sut.RetirarReservaAsync(1, 2, _ct);

            AssertQuantidade(dto, result.Value!);
            _serviceMock.Verify(s => s.RetirarReservaAsync(1, 2, _ct), Times.Once);
        }

        #endregion
    }
}