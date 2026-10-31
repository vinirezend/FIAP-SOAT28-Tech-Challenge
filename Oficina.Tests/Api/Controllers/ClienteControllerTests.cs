using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Api.DTO.Cliente;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Tests.Controllers
{
    public class ClienteControllerTests
    {
        private readonly Mock<IClienteService> _serviceMock = new();
        private readonly Mock<Microsoft.AspNetCore.Mvc.IUrlHelper> _urlMock = ControllerTestHelpers.CriarUrlHelperMock();
        private readonly ClienteController _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public ClienteControllerTests()
        {
            _sut = new ClienteController(_serviceMock.Object) { Url = _urlMock.Object };
        }

        #region Helpers

        private static ClienteDTO CriarDto(int id = 1, string nome = "João da Silva") => new()
        {
            IdCliente = id,
            Nome = nome,
            Documento = "52998224725",
            TipoDocumento = "CPF",
            Email = "joao@email.com",
            Telefone = "11987654321"
        };

        private static void AssertMapeado(ClienteDTO dto, ClienteResponse response)
        {
            Assert.Equal(dto.IdCliente, response.IdCliente);
            Assert.Equal(dto.Nome, response.Nome);
            Assert.Equal(dto.Documento, response.Documento);
            Assert.Equal(dto.TipoDocumento, response.TipoDocumento);
            Assert.Equal(dto.Email, response.Email);
            Assert.Equal(dto.Telefone, response.Telefone);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_Existente_DeveRetornarOk()
        {
            var dto = CriarDto();
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterPorIdAsync(1, _ct);

            var ok = Assert.IsType<Ok<ClienteResponse>>(result.Result);
            AssertMapeado(dto, ok.Value!);
        }

        [Fact]
        public async Task ObterPorIdAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((ClienteDTO?)null);

            var result = await _sut.ObterPorIdAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        #endregion

        #region ObterPorDocumentoAsync

        [Fact]
        public async Task ObterPorDocumentoAsync_Existente_DeveRetornarOk()
        {
            var dto = CriarDto();
            _serviceMock.Setup(s => s.ObterPorDocumentoAsync(dto.Documento, _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterPorDocumentoAsync(dto.Documento, _ct);

            var ok = Assert.IsType<Ok<ClienteResponse>>(result.Result);
            AssertMapeado(dto, ok.Value!);
        }

        [Fact]
        public async Task ObterPorDocumentoAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterPorDocumentoAsync("000", _ct)).ReturnsAsync((ClienteDTO?)null);

            var result = await _sut.ObterPorDocumentoAsync("000", _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        #endregion

        #region ObterTodosAsync

        [Fact]
        public async Task ObterTodosAsync_ComClientes_DeveRetornarTodosMapeados()
        {
            var dtos = new List<ClienteDTO> { CriarDto(1, "Cliente 1"), CriarDto(2, "Cliente 2") };
            _serviceMock.Setup(s => s.ObterTodosAsync(_ct)).ReturnsAsync(dtos);

            var result = await _sut.ObterTodosAsync(_ct);

            var itens = result.Value!.ToList();
            Assert.Equal(2, itens.Count);
            AssertMapeado(dtos[0], itens[0]);
            AssertMapeado(dtos[1], itens[1]);
        }

        [Fact]
        public async Task ObterTodosAsync_SemClientes_DeveRetornarOkVazio()
        {
            _serviceMock.Setup(s => s.ObterTodosAsync(_ct)).ReturnsAsync(new List<ClienteDTO>());

            var result = await _sut.ObterTodosAsync(_ct);

            Assert.Empty(result.Value!);
        }

        #endregion

        #region AdicionarAsync

        [Fact]
        public async Task AdicionarAsync_DeveEnviarDtoAoServiceERetornarCreated()
        {
            var request = new ClienteCreateRequest
            {
                Nome = "João da Silva",
                Documento = "52998224725",
                Email = "joao@email.com",
                Telefone = "11987654321"
            };
            var retorno = CriarDto(7);
            _serviceMock.Setup(s => s.AdicionarAsync(It.IsAny<ClienteDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AdicionarAsync(request, _ct);

            Assert.Equal(ControllerTestHelpers.LocationPadrao, result.Location);
            AssertMapeado(retorno, result.Value!);
            _serviceMock.Verify(s => s.AdicionarAsync(
                It.Is<ClienteDTO>(d => d.Nome == request.Nome
                                    && d.Documento == request.Documento
                                    && d.Email == request.Email
                                    && d.Telefone == request.Telefone),
                _ct), Times.Once);
            _urlMock.Verify(u => u.Action(It.Is<UrlActionContext>(c => c.Action == nameof(ClienteController.ObterPorIdAsync))), Times.Once);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_Existente_DeveEnviarDtoComIdDaRotaERetornarOk()
        {
            var request = new ClienteUpdateRequest { Nome = "Novo Nome", Email = "novo@email.com", Telefone = "11911112222" };
            var retorno = CriarDto(3, "Novo Nome");
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<ClienteDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AtualizarAsync(3, request, _ct);

            var ok = Assert.IsType<Ok<ClienteResponse>>(result.Result);
            AssertMapeado(retorno, ok.Value!);
            _serviceMock.Verify(s => s.AtualizarAsync(
                It.Is<ClienteDTO>(d => d.IdCliente == 3
                                    && d.Nome == request.Nome
                                    && d.Email == request.Email
                                    && d.Telefone == request.Telefone),
                _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<ClienteDTO>(), _ct)).ReturnsAsync((ClienteDTO?)null);

            var result = await _sut.AtualizarAsync(99, new ClienteUpdateRequest(), _ct);

            Assert.IsType<NotFound>(result.Result);
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
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((ClienteDTO?)null);

            var result = await _sut.DeletarAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
            _serviceMock.Verify(s => s.RemoverAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}