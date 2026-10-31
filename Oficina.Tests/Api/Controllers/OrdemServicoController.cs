using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Api.DTO.OrdemServico;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Tests.Controllers
{
    public class OrdemServicoControllerTests
    {
        private readonly Mock<IOrdemServicoService> _serviceMock = new();
        private readonly Mock<IUrlHelper> _urlMock = ControllerTestHelpers.CriarUrlHelperMock();
        private readonly OrdemServicoController _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public OrdemServicoControllerTests()
        {
            _sut = new OrdemServicoController(_serviceMock.Object) { Url = _urlMock.Object };
        }

        #region Helpers

        private static OrdemServicoDTO CriarOs(int id = 1) => new()
        {
            IdOs = id,
            IdVeiculo = 2,
            IdStatus = 3,
            Status = "Em diagnóstico",
            Valor = 150.5m
        };

        private static OrdemServicoServicoDTO CriarOsServico(int id = 1) => new()
        {
            IdOsServico = id,
            IdOs = 10,
            IdServico = 5,
            Quantidade = 2,
            ValorCobrado = 50m,
            ValorTotal = 100m
        };

        private static OrdemServicoInsumoDTO CriarOsInsumo(int id = 1) => new()
        {
            IdOsInsumo = id,
            IdOs = 10,
            IdInsumo = 6,
            Quantidade = 3,
            ValorCobrado = 20m,
            ValorTotal = 60m
        };

        private static void AssertOs(OrdemServicoDTO dto, OrdemServicoResponse r)
        {
            Assert.Equal(dto.IdOs, r.IdOs);
            Assert.Equal(dto.IdVeiculo, r.IdVeiculo);
            Assert.Equal(dto.DataInicio, r.DataInicio);
            Assert.Equal(dto.DataFim, r.DataFim);
            Assert.Equal(dto.Valor, r.Valor);
            Assert.Equal(dto.IdStatus, r.IdStatus);
            Assert.Equal(dto.Status, r.Status);
        }

        private static void AssertOsServico(OrdemServicoServicoDTO dto, OrdemServicoServicoResponse r)
        {
            Assert.Equal(dto.IdOsServico, r.IdOsServico);
            Assert.Equal(dto.IdOs, r.IdOs);
            Assert.Equal(dto.IdServico, r.IdServico);
            Assert.Equal(dto.Quantidade, r.Quantidade);
            Assert.Equal(dto.ValorCobrado, r.ValorCobrado);
            Assert.Equal(dto.ValorTotal, r.ValorTotal);
        }

        private static void AssertOsInsumo(OrdemServicoInsumoDTO dto, OrdemServicoInsumoResponse r)
        {
            Assert.Equal(dto.IdOsInsumo, r.IdOsInsumo);
            Assert.Equal(dto.IdOs, r.IdOs);
            Assert.Equal(dto.IdInsumo, r.IdInsumo);
            Assert.Equal(dto.Quantidade, r.Quantidade);
            Assert.Equal(dto.ValorCobrado, r.ValorCobrado);
            Assert.Equal(dto.ValorTotal, r.ValorTotal);
        }

        #endregion

        #region Consultas de OS

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarTodasMapeadas()
        {
            var dtos = new List<OrdemServicoDTO> { CriarOs(1), CriarOs(2) };
            _serviceMock.Setup(s => s.ObterTodosAsync(_ct)).ReturnsAsync(dtos);

            var result = await _sut.ObterTodosAsync(_ct);

            var itens = result.Value!.ToList();
            Assert.Equal(2, itens.Count);
            AssertOs(dtos[0], itens[0]);
            AssertOs(dtos[1], itens[1]);
        }

        [Fact]
        public async Task ObterPorIdAsync_Existente_DeveRetornarOk()
        {
            var dto = CriarOs();
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterPorIdAsync(1, _ct);

            var ok = Assert.IsType<Ok<OrdemServicoResponse>>(result.Result);
            AssertOs(dto, ok.Value!);
        }

        [Fact]
        public async Task ObterPorIdAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((OrdemServicoDTO?)null);

            var result = await _sut.ObterPorIdAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        [Fact]
        public async Task ObterPorVeiculoAsync_DeveRetornarOsDoVeiculo()
        {
            var dtos = new List<OrdemServicoDTO> { CriarOs(1) };
            _serviceMock.Setup(s => s.ObterPorVeiculoAsync(2, _ct)).ReturnsAsync(dtos);

            var result = await _sut.ObterPorVeiculoAsync(2, _ct);

            var itens = result.Value!.ToList();
            Assert.Single(itens);
            AssertOs(dtos[0], itens[0]);
        }

        #endregion

        #region ObterTempoMedioExecucaoAsync

        [Fact]
        public async Task ObterTempoMedioExecucaoAsync_ComTempo_DeveRetornarOkComHorasArredondadas()
        {
            var inicio = new DateOnly(2025, 1, 1);
            var fim = new DateOnly(2025, 1, 31);
            var dto = new TempoMedioExecucaoDTO { QuantidadeOrdens = 3, TempoMedio = TimeSpan.FromMinutes(100) };
            _serviceMock.Setup(s => s.ObterTempoMedioExecucaoAsync(inicio, fim, 5, _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterTempoMedioExecucaoAsync(inicio, fim, 5, _ct);

            var ok = Assert.IsType<Ok<TempoMedioExecucaoResponse>>(result.Result);
            Assert.Equal(3, ok.Value!.QuantidadeOrdens);
            Assert.Equal(dto.TempoMedio, ok.Value.TempoMedio);
            Assert.Equal((double?)1.67, ok.Value.TempoMedioEmHoras); // 100 min = 1,6666h -> 1,67
        }

        [Fact]
        public async Task ObterTempoMedioExecucaoAsync_SemTempo_DeveRetornarHorasNulas()
        {
            var dto = new TempoMedioExecucaoDTO { QuantidadeOrdens = 0, TempoMedio = null };
            _serviceMock.Setup(s => s.ObterTempoMedioExecucaoAsync(null, null, null, _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterTempoMedioExecucaoAsync(null, null, null, _ct);

            var ok = Assert.IsType<Ok<TempoMedioExecucaoResponse>>(result.Result);
            Assert.Null(ok.Value!.TempoMedio);
            Assert.Null(ok.Value.TempoMedioEmHoras);
        }

        [Fact]
        public async Task ObterTempoMedioExecucaoAsync_ServicoInexistente_DeveRetornarNotFound()
        {
            _serviceMock
                .Setup(s => s.ObterTempoMedioExecucaoAsync(null, null, 99, _ct))
                .ReturnsAsync((TempoMedioExecucaoDTO?)null);

            var result = await _sut.ObterTempoMedioExecucaoAsync(null, null, 99, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        #endregion

        #region AdicionarAsync / AtualizarAsync

        [Fact]
        public async Task AdicionarAsync_DeveEnviarVeiculoERetornarCreated()
        {
            var retorno = CriarOs(7);
            _serviceMock.Setup(s => s.AdicionarAsync(It.IsAny<OrdemServicoDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AdicionarAsync(new OrdemServicoRequest { IdVeiculo = 2 }, _ct);

            Assert.Equal(ControllerTestHelpers.LocationPadrao, result.Location);
            AssertOs(retorno, result.Value!);
            _serviceMock.Verify(s => s.AdicionarAsync(It.Is<OrdemServicoDTO>(d => d.IdVeiculo == 2), _ct), Times.Once);
            _urlMock.Verify(u => u.Action(It.Is<UrlActionContext>(c => c.Action == nameof(OrdemServicoController.ObterPorIdAsync))), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_Existente_DeveEnviarIdDaRotaERetornarOk()
        {
            var retorno = CriarOs(3);
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<OrdemServicoDTO>(), _ct)).ReturnsAsync(retorno);

            var result = await _sut.AtualizarAsync(3, new OrdemServicoRequest { IdVeiculo = 8 }, _ct);

            var ok = Assert.IsType<Ok<OrdemServicoResponse>>(result.Result);
            AssertOs(retorno, ok.Value!);
            _serviceMock.Verify(s => s.AtualizarAsync(It.Is<OrdemServicoDTO>(d => d.IdOs == 3 && d.IdVeiculo == 8), _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AtualizarAsync(It.IsAny<OrdemServicoDTO>(), _ct)).ReturnsAsync((OrdemServicoDTO?)null);

            var result = await _sut.AtualizarAsync(99, new OrdemServicoRequest { IdVeiculo = 1 }, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        #endregion

        #region Status

        [Fact]
        public async Task ObterHistoricoStatusAsync_Existente_DeveRetornarOkMapeado()
        {
            var historico = new List<OrdemServicoStatusDTO>
        {
            new() { IdOsStatus = 1, IdStatus = 2, Status = "Recebida", IdColaborador = 9 }
        };
            _serviceMock.Setup(s => s.ObterHistoricoStatusAsync(1, _ct)).ReturnsAsync(historico);

            var result = await _sut.ObterHistoricoStatusAsync(1, _ct);

            var ok = Assert.IsType<Ok<IEnumerable<OrdemServicoStatusResponse>>>(result.Result);
            var item = Assert.Single(ok.Value!);
            Assert.Equal(historico[0].IdOsStatus, item.IdOsStatus);
            Assert.Equal(historico[0].IdStatus, item.IdStatus);
            Assert.Equal(historico[0].Status, item.Status);
            Assert.Equal(historico[0].IdColaborador, item.IdColaborador);
            Assert.Equal(historico[0].DataInclusao, item.DataInclusao);
        }

        [Fact]
        public async Task ObterHistoricoStatusAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterHistoricoStatusAsync(99, _ct)).ReturnsAsync(() => null!);

            var result = await _sut.ObterHistoricoStatusAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        [Fact]
        public async Task AlterarStatusAsync_Existente_DeveRetornarOk()
        {
            var retorno = CriarOs(1);
            _serviceMock.Setup(s => s.AlterarStatusAsync(1, 4, _ct)).ReturnsAsync(retorno);

            var result = await _sut.AlterarStatusAsync(1, new AlterarStatusRequest { IdStatus = 4 }, _ct);

            var ok = Assert.IsType<Ok<OrdemServicoResponse>>(result.Result);
            AssertOs(retorno, ok.Value!);
        }

        [Fact]
        public async Task AlterarStatusAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AlterarStatusAsync(99, 4, _ct)).ReturnsAsync((OrdemServicoDTO?)null);

            var result = await _sut.AlterarStatusAsync(99, new AlterarStatusRequest { IdStatus = 4 }, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        #endregion

        #region Serviços da OS

        [Fact]
        public async Task ObterServicosAsync_Existente_DeveRetornarOkMapeado()
        {
            var servicos = new List<OrdemServicoServicoDTO> { CriarOsServico() };
            _serviceMock.Setup(s => s.ObterServicosAsync(10, _ct)).ReturnsAsync(servicos);

            var result = await _sut.ObterServicosAsync(10, _ct);

            var ok = Assert.IsType<Ok<IEnumerable<OrdemServicoServicoResponse>>>(result.Result);
            AssertOsServico(servicos[0], Assert.Single(ok.Value!));
        }

        [Fact]
        public async Task ObterServicosAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterServicosAsync(99, _ct)).ReturnsAsync(() => null!);

            var result = await _sut.ObterServicosAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        [Fact]
        public async Task AdicionarServicoAsync_Existente_DeveRetornarCreated()
        {
            var retorno = CriarOsServico();
            _serviceMock.Setup(s => s.AdicionarServicoAsync(10, 5, 2, _ct)).ReturnsAsync(retorno);

            var result = await _sut.AdicionarServicoAsync(10, new AdicionarServicoRequest { IdServico = 5, Quantidade = 2 }, _ct);

            var created = Assert.IsType<Created<OrdemServicoServicoResponse>>(result.Result);
            Assert.Equal(ControllerTestHelpers.LocationPadrao, created.Location);
            AssertOsServico(retorno, created.Value!);
            _urlMock.Verify(u => u.Action(It.Is<UrlActionContext>(c => c.Action == nameof(OrdemServicoController.ObterServicosAsync))), Times.Once);
        }

        [Fact]
        public async Task AdicionarServicoAsync_OsInexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AdicionarServicoAsync(99, 5, 2, _ct)).ReturnsAsync((OrdemServicoServicoDTO?)null);

            var result = await _sut.AdicionarServicoAsync(99, new AdicionarServicoRequest { IdServico = 5, Quantidade = 2 }, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        [Fact]
        public async Task AlterarQuantidadeServicoAsync_Existente_DeveRetornarOk()
        {
            var retorno = CriarOsServico();
            _serviceMock.Setup(s => s.AlterarQuantidadeServicoAsync(10, 1, 4, _ct)).ReturnsAsync(retorno);

            var result = await _sut.AlterarQuantidadeServicoAsync(10, 1, new AlterarQuantidadeServicoRequest { Quantidade = 4 }, _ct);

            var ok = Assert.IsType<Ok<OrdemServicoServicoResponse>>(result.Result);
            AssertOsServico(retorno, ok.Value!);
        }

        [Fact]
        public async Task AlterarQuantidadeServicoAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AlterarQuantidadeServicoAsync(10, 99, 4, _ct)).ReturnsAsync((OrdemServicoServicoDTO?)null);

            var result = await _sut.AlterarQuantidadeServicoAsync(10, 99, new AlterarQuantidadeServicoRequest { Quantidade = 4 }, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        [Fact]
        public async Task RemoverServicoAsync_DeveDelegarERetornarNoContent()
        {
            var result = await _sut.RemoverServicoAsync(10, 1, _ct);

            Assert.IsType<NoContent>(result);
            _serviceMock.Verify(s => s.RemoverServicoAsync(10, 1, _ct), Times.Once);
        }

        #endregion

        #region Insumos da OS

        [Fact]
        public async Task ObterInsumosAsync_Existente_DeveRetornarOkMapeado()
        {
            var insumos = new List<OrdemServicoInsumoDTO> { CriarOsInsumo() };
            _serviceMock.Setup(s => s.ObterInsumosAsync(10, _ct)).ReturnsAsync(insumos);

            var result = await _sut.ObterInsumosAsync(10, _ct);

            var ok = Assert.IsType<Ok<IEnumerable<OrdemServicoInsumoResponse>>>(result.Result);
            AssertOsInsumo(insumos[0], Assert.Single(ok.Value!));
        }

        [Fact]
        public async Task ObterInsumosAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterInsumosAsync(99, _ct)).ReturnsAsync(() => null!);

            var result = await _sut.ObterInsumosAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        [Fact]
        public async Task AdicionarInsumoAsync_Existente_DeveRetornarCreated()
        {
            var retorno = CriarOsInsumo();
            _serviceMock.Setup(s => s.AdicionarInsumoAsync(10, 6, 3, _ct)).ReturnsAsync(retorno);

            var result = await _sut.AdicionarInsumoAsync(10, new AdicionarInsumoRequest { IdInsumo = 6, Quantidade = 3 }, _ct);

            var created = Assert.IsType<Created<OrdemServicoInsumoResponse>>(result.Result);
            Assert.Equal(ControllerTestHelpers.LocationPadrao, created.Location);
            AssertOsInsumo(retorno, created.Value!);
            _urlMock.Verify(u => u.Action(It.Is<UrlActionContext>(c => c.Action == nameof(OrdemServicoController.ObterInsumosAsync))), Times.Once);
        }

        [Fact]
        public async Task AdicionarInsumoAsync_OsInexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AdicionarInsumoAsync(99, 6, 3, _ct)).ReturnsAsync((OrdemServicoInsumoDTO?)null);

            var result = await _sut.AdicionarInsumoAsync(99, new AdicionarInsumoRequest { IdInsumo = 6, Quantidade = 3 }, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        [Fact]
        public async Task AlterarQuantidadeInsumoAsync_Existente_DeveRetornarOk()
        {
            var retorno = CriarOsInsumo();
            _serviceMock.Setup(s => s.AlterarQuantidadeInsumoAsync(10, 1, 5, _ct)).ReturnsAsync(retorno);

            var result = await _sut.AlterarQuantidadeInsumoAsync(10, 1, new AlterarQuantidadeInsumoRequest { Quantidade = 5 }, _ct);

            var ok = Assert.IsType<Ok<OrdemServicoInsumoResponse>>(result.Result);
            AssertOsInsumo(retorno, ok.Value!);
        }

        [Fact]
        public async Task AlterarQuantidadeInsumoAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.AlterarQuantidadeInsumoAsync(10, 99, 5, _ct)).ReturnsAsync((OrdemServicoInsumoDTO?)null);

            var result = await _sut.AlterarQuantidadeInsumoAsync(10, 99, new AlterarQuantidadeInsumoRequest { Quantidade = 5 }, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        [Fact]
        public async Task RemoverInsumoAsync_DeveDelegarERetornarNoContent()
        {
            var result = await _sut.RemoverInsumoAsync(10, 1, _ct);

            Assert.IsType<NoContent>(result);
            _serviceMock.Verify(s => s.RemoverInsumoAsync(10, 1, _ct), Times.Once);
        }

        #endregion

        #region DeletarAsync

        [Fact]
        public async Task DeletarAsync_Inexistente_DeveRetornarNotFoundENaoRemover()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(99, _ct)).ReturnsAsync((OrdemServicoDTO?)null);

            var result = await _sut.DeletarAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
            _serviceMock.Verify(s => s.RemoverAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeletarAsync_Existente_DeveRemoverERetornarNoContent()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(CriarOs());

            var result = await _sut.DeletarAsync(1, _ct);

            Assert.IsType<NoContent>(result.Result);
            _serviceMock.Verify(s => s.RemoverAsync(1, _ct), Times.Once);
        }

        [Fact]
        public async Task DeletarAsync_StatusNaoPermiteRemocao_DeveRetornarProblemaConflict()
        {
            _serviceMock.Setup(s => s.ObterPorIdAsync(1, _ct)).ReturnsAsync(CriarOs());
            _serviceMock.Setup(s => s.RemoverAsync(1, _ct)).ThrowsAsync(new InvalidOperationException("OS em execução não pode ser removida."));

            var result = await _sut.DeletarAsync(1, _ct);

            var problem = Assert.IsType<ProblemHttpResult>(result.Result);
            Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
            Assert.Equal("OS em execução não pode ser removida.", problem.ProblemDetails.Detail);
        }

        #endregion
    }
}