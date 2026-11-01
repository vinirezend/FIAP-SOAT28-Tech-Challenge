using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Api.DTO.Colaborador;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Tests.Controllers
{
    public class ColaboradorControllerTests
    {
        private readonly Mock<IColaboradorService> _serviceMock = new();
        private readonly Mock<IUrlHelper> _urlMock = ControllerTestHelpers.CriarUrlHelperMock();
        private readonly ColaboradorController _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public ColaboradorControllerTests()
        {
            _sut = new ColaboradorController(_serviceMock.Object) { Url = _urlMock.Object };
        }

        #region Helpers

        // IdUsuario fica com o valor padrão (não sei o tipo) e é sempre comparado por igualdade com a origem.
        private static ColaboradorDTO CriarDto(int id = 1, string nome = "Carlos Souza") => new()
        {
            IdColaborador = id,
            Nome = nome,
            Cpf = "52998224725",
            Email = "carlos@oficina.com",
            Ativo = true,
            IdDepartamento = 2
        };

        private static void AssertMapeado(ColaboradorDTO dto, ColaboradorResponse r)
        {
            Assert.Equal(dto.IdColaborador, r.IdColaborador);
            Assert.Equal(dto.Nome, r.Nome);
            Assert.Equal(dto.Cpf, r.Cpf);
            Assert.Equal(dto.Email, r.Email);
            Assert.Equal(dto.Ativo, r.Ativo);
            Assert.Equal(dto.IdDepartamento, r.IdDepartamento);
            Assert.Equal(dto.IdUsuario, r.IdUsuario);
        }

        #endregion

        #region ObterTodosAsync

        [Fact]
        public async Task ObterTodosAsync_ComColaboradores_DeveRetornarTodosMapeados()
        {
            var dtos = new List<ColaboradorDTO> { CriarDto(1, "Colaborador 1"), CriarDto(2, "Colaborador 2") };
            _serviceMock.Setup(s => s.ObterTodosAsync(_ct)).ReturnsAsync(dtos);

            var result = await _sut.ObterTodosAsync(_ct);

            var itens = result.Value!.ToList();
            Assert.Equal(2, itens.Count);
            AssertMapeado(dtos[0], itens[0]);
            AssertMapeado(dtos[1], itens[1]);
        }

        [Fact]
        public async Task ObterTodosAsync_SemColaboradores_DeveRetornarOkVazio()
        {
            _serviceMock.Setup(s => s.ObterTodosAsync(_ct)).ReturnsAsync(new List<ColaboradorDTO>());

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

            var ok = Assert.IsType<Ok<ColaboradorResponse>>(result.Result);
            AssertMapeado(dto, ok.Value!);
        }

        [Fact]
        public async Task ObterPorIdAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((ColaboradorDTO?)null);

            var result = await _sut.ObterPorIdAsync(99, _ct);

            Assert.IsType<NotFound<string>>(TypedResults.NotFound("Não encontrado"));
        }

        #endregion

        #region AdicionarAsync

        [Fact]
        public async Task AdicionarAsync_DeveEnviarDtoAoServiceERetornarCreated()
        {
            var request = new ColaboradorCreateRequest
            {
                Nome = "Carlos Souza",
                Cpf = "52998224725",
                Email = "carlos@oficina.com",
                IdDepartamento = 2
            };
            var retorno = CriarDto(7);
            _serviceMock.Setup(s => s.AdicionarAsync(It.IsAny<ColaboradorDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AdicionarAsync(request, _ct);

            Assert.Equal(ControllerTestHelpers.LocationPadrao, result.Location);
            AssertMapeado(retorno, result.Value!);
            _serviceMock.Verify(s => s.AdicionarAsync(
                It.Is<ColaboradorDTO>(d => d.Nome == request.Nome
                                        && d.Cpf == request.Cpf
                                        && d.Email == request.Email
                                        && d.IdDepartamento == request.IdDepartamento
                                        && d.IdUsuario == request.IdUsuario),
                _ct), Times.Once);
            _urlMock.Verify(u => u.Action(It.Is<UrlActionContext>(c => c.Action == nameof(ColaboradorController.ObterPorIdAsync))), Times.Once);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_Existente_DeveEnviarDtoComIdDaRotaERetornarOk()
        {
            var request = new ColaboradorUpdateRequest
            {
                Nome = "Novo Nome",
                Email = "novo@oficina.com",
                IdDepartamento = 3,
                Ativo = false
            };
            var retorno = CriarDto(3, "Novo Nome");
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<ColaboradorDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AtualizarAsync(3, request, _ct);

            var ok = Assert.IsType<Ok<ColaboradorResponse>>(result.Result);
            AssertMapeado(retorno, ok.Value!);
            _serviceMock.Verify(s => s.AtualizarAsync(
                It.Is<ColaboradorDTO>(d => d.IdColaborador == 3
                                        && d.Nome == request.Nome
                                        && d.Email == request.Email
                                        && d.IdDepartamento == request.IdDepartamento
                                        && d.IdUsuario == request.IdUsuario
                                        && d.Ativo == request.Ativo),
                _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<ColaboradorDTO>(), _ct)).ReturnsAsync((ColaboradorDTO?)null);

            var result = await _sut.AtualizarAsync(99, new ColaboradorUpdateRequest() { Nome = "Nome Teste", Email = "emailTeste@teste.com" }, _ct);

            Assert.IsType<NotFound<string>>(TypedResults.NotFound("Não encontrado"));
        }

        #endregion

        #region DeletarAsync

        [Fact]
        public async Task DeletarAsync_Inexistente_DeveRetornarNotFoundENaoRemover()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((ColaboradorDTO?)null);

            var result = await _sut.DeletarAsync(99, _ct);

            Assert.IsType<NotFound<string>>(TypedResults.NotFound("Não encontrado"));
            _serviceMock.Verify(s => s.RemoverAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeletarAsync_Existente_DeveRemoverERetornarNoContent()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(CriarDto());

            var result = await _sut.DeletarAsync(1, _ct);

            Assert.IsType<NoContent>(result.Result);
            _serviceMock.Verify(s => s.RemoverAsync(1, _ct), Times.Once);
        }

        [Fact]
        public async Task DeletarAsync_ComRegistrosVinculados_DeveRetornarProblemaConflict()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(CriarDto());
            _serviceMock.Setup(s => s.RemoverAsync(1, _ct)).ThrowsAsync(new InvalidOperationException("Colaborador possui registros vinculados."));

            var result = await _sut.DeletarAsync(1, _ct);

            var problem = Assert.IsType<ProblemHttpResult>(result.Result);
            Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
            Assert.Equal("Colaborador possui registros vinculados.", problem.ProblemDetails.Detail);
        }

        #endregion
    }
}