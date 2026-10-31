using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Api.DTO.Orcamento;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using RetornoAprovacao = Oficina.Application.DTOs.OrdemServicoDTO;

namespace Oficina.Api.Tests.Controllers
{
    public class OrcamentoControllerTests
    {
        private const int StatusAguardandoAprovacao = (int)Oficina.Domain.Enum.StatusOs.AguardandoAprovacao;

        private readonly Mock<IOrcamentoService> _orcamentoServiceMock = new();
        private readonly Mock<IOrdemServicoService> _ordemServicoServiceMock = new();
        private readonly OrcamentoController _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public OrcamentoControllerTests()
        {
            _sut = new OrcamentoController(_orcamentoServiceMock.Object, _ordemServicoServiceMock.Object);

            // PaginaConfirmacaoAsync lê Request.PathBase / Path / QueryString
            var httpContext = ControllerTestHelpers.ConfigurarHttpContext(_sut);
            httpContext.Request.Path = "/api/Orcamento/10/aprovar";
            httpContext.Request.QueryString = new QueryString("?expira=123&assinatura=abc");
        }

        #region Helpers

        private static OrcamentoDTO CriarOrcamento() => new()
        {
            IdOs = 10,
            IdStatus = 2,
            Status = "Aguardando aprovação",
            ClienteNome = "João da Silva",
            ClienteEmail = "joao@email.com",
            Placa = "ABC1D23",
            Marca = "Volkswagen",
            Modelo = "Gol",
            Ano = 2020,
            Servicos = new List<OrcamentoItemDTO>
        {
            new() { Nome = "Troca de óleo", Quantidade = 1, ValorUnitario = 100m, ValorTotal = 100m }
        },
            Insumos = new List<OrcamentoItemDTO>
        {
            new() { Nome = "Óleo 5W30", Quantidade = 4, ValorUnitario = 12m, ValorTotal = 48m }
        },
            ValorTotal = 148m
        };

        private static OrdemServicoDTO CriarOs(int idStatus) => new()
        {
            IdOs = 10,
            IdVeiculo = 2,
            IdStatus = idStatus,
            Status = "Status",
            Valor = 148m
        };

        private static void AssertItem(OrcamentoItemDTO dto, OrcamentoItemResponse r)
        {
            Assert.Equal(dto.Nome, r.Nome);
            Assert.Equal(dto.Quantidade, r.Quantidade);
            Assert.Equal(dto.ValorUnitario, r.ValorUnitario);
            Assert.Equal(dto.ValorTotal, r.ValorTotal);
        }

        private Task<ContentResult> ConfirmarAsync(bool aprovar, int idOs) =>
            aprovar ? _sut.ConfirmarAprovacaoAsync(idOs, _ct) : _sut.ConfirmarRecusaAsync(idOs, _ct);

        private static void AssertHtmlComConteudo(ContentResult result) =>
            Assert.False(string.IsNullOrWhiteSpace(result.Content));

        #endregion

        #region ObterAsync

        [Fact]
        public async Task ObterAsync_Existente_DeveRetornarOkMapeado()
        {
            var dto = CriarOrcamento();
            _orcamentoServiceMock.Setup(s => s.ObterAsync(10, _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterAsync(10, _ct);

            var ok = Assert.IsType<Ok<OrcamentoResponse>>(result.Result);
            var r = ok.Value!;
            Assert.Equal(dto.IdOs, r.IdOs);
            Assert.Equal(dto.IdStatus, r.IdStatus);
            Assert.Equal(dto.Status, r.Status);
            Assert.Equal(dto.ClienteNome, r.ClienteNome);
            Assert.Equal(dto.ClienteEmail, r.ClienteEmail);
            Assert.Equal(dto.Placa, r.Placa);
            Assert.Equal(dto.Marca, r.Marca);
            Assert.Equal(dto.Modelo, r.Modelo);
            Assert.Equal(dto.Ano, r.Ano);
            Assert.Equal(dto.ValorTotal, r.ValorTotal);
            AssertItem(dto.Servicos.Single(), Assert.Single(r.Servicos));
            AssertItem(dto.Insumos.Single(), Assert.Single(r.Insumos));
        }

        [Fact]
        public async Task ObterAsync_Inexistente_DeveRetornarNotFound()
        {
            _orcamentoServiceMock.Setup(s => s.ObterAsync(99, _ct)).ReturnsAsync((OrcamentoDTO?)null);

            var result = await _sut.ObterAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        #endregion

        #region EnviarAsync

        [Fact]
        public async Task EnviarAsync_Existente_DeveRetornarOkComLink()
        {
            var link = new LinkAssinadoDTO();
            _orcamentoServiceMock.Setup(s => s.EnviarAsync(10, 30, _ct)).ReturnsAsync(link);

            var result = await _sut.EnviarAsync(10, 30, _ct);

            var ok = Assert.IsType<Ok<LinkAssinadoDTO>>(result.Result);
            Assert.Same(link, ok.Value);
        }

        [Fact]
        public async Task EnviarAsync_SemMinutosValidade_DeveRepassarNulo()
        {
            _orcamentoServiceMock.Setup(s => s.EnviarAsync(10, null, _ct)).ReturnsAsync(new LinkAssinadoDTO());

            var result = await _sut.EnviarAsync(10, null, _ct);

            Assert.IsType<Ok<LinkAssinadoDTO>>(result.Result);
            _orcamentoServiceMock.Verify(s => s.EnviarAsync(10, null, _ct), Times.Once);
        }

        [Fact]
        public async Task EnviarAsync_Inexistente_DeveRetornarNotFound()
        {
            _orcamentoServiceMock.Setup(s => s.EnviarAsync(99, null, _ct)).ReturnsAsync((LinkAssinadoDTO?)null);

            var result = await _sut.EnviarAsync(99, null, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        #endregion

        #region ConfirmarAprovacaoAsync / ConfirmarRecusaAsync (GET)

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Confirmar_OsInexistente_DeveRetornarPagina404(bool aprovar)
        {
            _ordemServicoServiceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((OrdemServicoDTO?)null);

            var result = await ConfirmarAsync(aprovar, 99);

            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
            AssertHtmlComConteudo(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Confirmar_OsNaoAguardandoAprovacao_DeveRetornarPaginaJaRespondida(bool aprovar)
        {
            _ordemServicoServiceMock
                .Setup(s => s.ObterPorIdAsync(10, _ct))
                .ReturnsAsync(CriarOs(StatusAguardandoAprovacao + 1000));

            var result = await ConfirmarAsync(aprovar, 10);

            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            AssertHtmlComConteudo(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Confirmar_OsAguardandoAprovacao_DeveRetornarPaginaDeConfirmacao(bool aprovar)
        {
            _ordemServicoServiceMock
                .Setup(s => s.ObterPorIdAsync(10, _ct))
                .ReturnsAsync(CriarOs(StatusAguardandoAprovacao));

            var result = await ConfirmarAsync(aprovar, 10);

            Assert.True(result.StatusCode is null or StatusCodes.Status200OK);
            AssertHtmlComConteudo(result);
        }

        #endregion

        #region AprovarAsync (POST)

        [Fact]
        public async Task AprovarAsync_Sucesso_DeveRetornarPagina200()
        {
            _orcamentoServiceMock.Setup(s => s.AprovarAsync(10, _ct)).ReturnsAsync(new RetornoAprovacao());

            var result = await _sut.AprovarAsync(10, _ct);

            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            AssertHtmlComConteudo(result);
        }

        [Fact]
        public async Task AprovarAsync_OsInexistente_DeveRetornarPagina404()
        {
            _orcamentoServiceMock.Setup(s => s.AprovarAsync(99, _ct)).ReturnsAsync((RetornoAprovacao?)null);

            var result = await _sut.AprovarAsync(99, _ct);

            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async Task AprovarAsync_OperacaoInvalida_DeveRetornarPagina400ComMensagem()
        {
            _orcamentoServiceMock.Setup(s => s.AprovarAsync(10, _ct)).ThrowsAsync(new InvalidOperationException("Falha-aprovacao-xyz"));

            var result = await _sut.AprovarAsync(10, _ct);

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.Contains("Falha-aprovacao-xyz", result.Content);
        }

        #endregion

        #region RecusarAsync (POST)

        [Fact]
        public async Task RecusarAsync_Sucesso_DeveRetornarPagina200()
        {
            _orcamentoServiceMock.Setup(s => s.RecusarAsync(10, _ct)).ReturnsAsync(new RetornoAprovacao());

            var result = await _sut.RecusarAsync(10, _ct);

            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            AssertHtmlComConteudo(result);
        }

        [Fact]
        public async Task RecusarAsync_OsInexistente_DeveRetornarPagina404()
        {
            _orcamentoServiceMock.Setup(s => s.RecusarAsync(99, _ct)).ReturnsAsync((RetornoAprovacao?)null);

            var result = await _sut.RecusarAsync(99, _ct);

            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async Task RecusarAsync_OperacaoInvalida_DeveRetornarPagina400ComMensagem()
        {
            _orcamentoServiceMock.Setup(s => s.RecusarAsync(10, _ct)).ThrowsAsync(new InvalidOperationException("Falha-recusa-xyz"));

            var result = await _sut.RecusarAsync(10, _ct);

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.Contains("Falha-recusa-xyz", result.Content);
        }

        #endregion
    }
}