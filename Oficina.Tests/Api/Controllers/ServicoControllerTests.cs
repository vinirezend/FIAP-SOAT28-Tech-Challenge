using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Api.DTO.Servico;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Tests.Controllers
{
    public class ServicoControllerTests
    {
        private readonly Mock<IServicoService> _serviceMock = new();
        private readonly Mock<IUrlHelper> _urlMock = ControllerTestHelpers.CriarUrlHelperMock();
        private readonly ServicoController _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public ServicoControllerTests()
        {
            _sut = new ServicoController(_serviceMock.Object) { Url = _urlMock.Object };
        }

        #region Helpers

        private static ServicoDTO CriarDto(int id = 1, string nome = "Troca de óleo") => new()
        {
            IdServico = id,
            Nome = nome,
            Descricao = "Troca de óleo e filtro",
            ValorEstipulado = 150m,
            Ativo = true
        };

        private static void AssertMapeado(ServicoDTO dto, ServicoResponse response)
        {
            Assert.Equal(dto.IdServico, response.IdServico);
            Assert.Equal(dto.Nome, response.Nome);
            Assert.Equal(dto.Descricao, response.Descricao);
            Assert.Equal(dto.ValorEstipulado, response.ValorEstipulado);
            Assert.Equal(dto.Ativo, response.Ativo);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_Existente_DeveRetornarOk()
        {
            var dto = CriarDto();
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterPorIdAsync(1, _ct);

            var ok = Assert.IsType<Ok<ServicoResponse>>(result.Result);
            AssertMapeado(dto, ok.Value!);
        }

        [Fact]
        public async Task ObterPorIdAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((ServicoDTO?)null);

            var result = await _sut.ObterPorIdAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        #endregion

        #region ObterTodosAsync / PesquisarAsync

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarTodosMapeados()
        {
            var dtos = new List<ServicoDTO> { CriarDto(1, "Serviço 1"), CriarDto(2, "Serviço 2") };
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
            var dtos = new List<ServicoDTO> { CriarDto(1, "Troca A") };
            _serviceMock.Setup(s => s.PesquisarAsync("troca", _ct)).ReturnsAsync(dtos);

            var result = await _sut.PesquisarAsync("troca", _ct);

            var itens = result.Value!.ToList();
            Assert.Single(itens);
            AssertMapeado(dtos[0], itens[0]);
            _serviceMock.Verify(s => s.PesquisarAsync("troca", _ct), Times.Once);
        }

        [Fact]
        public async Task PesquisarAsync_SemResultados_DeveRetornarOkVazio()
        {
            _serviceMock.Setup(s => s.PesquisarAsync("x", _ct)).ReturnsAsync(new List<ServicoDTO>());

            var result = await _sut.PesquisarAsync("x", _ct);

            Assert.Empty(result.Value!);
        }

        #endregion

        #region AdicionarAsync

        [Fact]
        public async Task AdicionarAsync_DeveEnviarDtoAtivoERetornarCreated()
        {
            var request = new ServicoCreateRequest { Nome = "Alinhamento", Descricao = "Alinhamento 3D", ValorEstipulado = 120m };
            var retorno = CriarDto(7, "Alinhamento");
            _serviceMock.Setup(s => s.AdicionarAsync(It.IsAny<ServicoDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AdicionarAsync(request, _ct);

            Assert.Equal(ControllerTestHelpers.LocationPadrao, result.Location);
            AssertMapeado(retorno, result.Value!);
            _serviceMock.Verify(s => s.AdicionarAsync(
                It.Is<ServicoDTO>(d => d.Nome == request.Nome
                                    && d.Descricao == request.Descricao
                                    && d.ValorEstipulado == request.ValorEstipulado
                                    && d.Ativo),
                _ct), Times.Once);
            _urlMock.Verify(u => u.Action(It.Is<UrlActionContext>(c => c.Action == nameof(ServicoController.ObterPorIdAsync))), Times.Once);
        }

        [Fact]
        public async Task AdicionarAsync_DescricaoNula_DeveEnviarStringVazia()
        {
            var request = new ServicoCreateRequest { Nome = "Alinhamento", Descricao = null, ValorEstipulado = 120m };
            _serviceMock.Setup(s => s.AdicionarAsync(It.IsAny<ServicoDTO>(), _ct)).ReturnsAsync(CriarDto());

            await _sut.AdicionarAsync(request, _ct);

            _serviceMock.Verify(s => s.AdicionarAsync(It.Is<ServicoDTO>(d => d.Descricao == string.Empty), _ct), Times.Once);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_Existente_DeveEnviarDtoComIdDaRotaERetornarOk()
        {
            var request = new ServicoUpdateRequest { Nome = "Novo", Descricao = "Nova desc", ValorEstipulado = 200m, Ativo = false };
            var retorno = CriarDto(3, "Novo");
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<ServicoDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AtualizarAsync(3, request, _ct);

            var ok = Assert.IsType<Ok<ServicoResponse>>(result.Result);
            AssertMapeado(retorno, ok.Value!);
            _serviceMock.Verify(s => s.AtualizarAsync(
                It.Is<ServicoDTO>(d => d.IdServico == 3
                                    && d.Nome == request.Nome
                                    && d.Descricao == request.Descricao
                                    && d.ValorEstipulado == request.ValorEstipulado
                                    && d.Ativo == request.Ativo),
                _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_DescricaoNula_DeveEnviarStringVazia()
        {
            var request = new ServicoUpdateRequest { Nome = "Novo", Descricao = null, ValorEstipulado = 200m, Ativo = true };
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<ServicoDTO>(), _ct)).ReturnsAsync(CriarDto());

            await _sut.AtualizarAsync(1, request, _ct);

            _serviceMock.Verify(s => s.AtualizarAsync(It.Is<ServicoDTO>(d => d.Descricao == string.Empty), _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<ServicoDTO>(), _ct)).ReturnsAsync((ServicoDTO?)null);

            var result = await _sut.AtualizarAsync(99, new ServicoUpdateRequest(), _ct);

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
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((ServicoDTO?)null);

            var result = await _sut.DeletarAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
            _serviceMock.Verify(s => s.RemoverAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}