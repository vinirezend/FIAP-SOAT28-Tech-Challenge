using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Api.DTO.AcompanhamentoOrdemServico;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Tests.Controllers
{
    public class AcompanhamentoOrdemServicoControllerTests
    {
        private readonly Mock<IAcompanhamentoOrdemServicoService> _serviceMock = new();
        private readonly AcompanhamentoOrdemServicoController _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public AcompanhamentoOrdemServicoControllerTests()
        {
            _sut = new AcompanhamentoOrdemServicoController(_serviceMock.Object);
        }

        #region Helpers

        private static AcompanhamentoOrdemServicoDTO CriarDto() => new()
        {
            IdOs = 10,
            IdStatus = 2,
            Status = "EmExecucao",
            DescricaoStatus = "Em execução",
            ClienteNome = "João da Silva",
            Placa = "ABC1D23",
            Marca = "Volkswagen",
            Modelo = "Gol",
            Ano = 2020,
            ValorTotal = 148m,
            Etapas = new List<AcompanhamentoOrdemServicoEtapaDTO>
        {
            new() { IdStatus = 1, Nome = "Recebida" },
            new() { IdStatus = 2, Nome = "Em execução" }
        }
        };

        #endregion

        #region ObterAsync

        [Fact]
        public async Task ObterAsync_Existente_DeveRetornarOkMapeado()
        {
            var dto = CriarDto();
            _serviceMock.Setup(s => s.ObterAsync(10, _ct)).ReturnsAsync(dto);

            var result = await _sut.ObterAsync(10, _ct);

            var ok = Assert.IsType<Ok<AcompanhamentoOrdemServicoResponse>>(result.Result);
            var r = ok.Value!;
            Assert.Equal(dto.IdOs, r.IdOs);
            Assert.Equal(dto.IdStatus, r.IdStatus);
            Assert.Equal(dto.Status, r.Status);
            Assert.Equal(dto.DescricaoStatus, r.DescricaoStatus);
            Assert.Equal(dto.ClienteNome, r.ClienteNome);
            Assert.Equal(dto.Placa, r.Placa);
            Assert.Equal(dto.Marca, r.Marca);
            Assert.Equal(dto.Modelo, r.Modelo);
            Assert.Equal(dto.Ano, r.Ano);
            Assert.Equal(dto.DataInicio, r.DataInicio);
            Assert.Equal(dto.DataFim, r.DataFim);
            Assert.Equal(dto.ValorTotal, r.ValorTotal);

            var etapasDto = dto.Etapas.ToList();
            var etapas = r.Etapas.ToList();
            Assert.Equal(etapasDto.Count, etapas.Count);
            for (var i = 0; i < etapasDto.Count; i++)
            {
                Assert.Equal(etapasDto[i].IdStatus, etapas[i].IdStatus);
                Assert.Equal(etapasDto[i].Nome, etapas[i].Nome);
                Assert.Equal(etapasDto[i].Situacao.ToString(), etapas[i].Situacao);
                Assert.Equal(etapasDto[i].Data, etapas[i].Data);
            }
        }

        [Fact]
        public async Task ObterAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.ObterAsync(99, _ct)).ReturnsAsync((AcompanhamentoOrdemServicoDTO?)null);

            var result = await _sut.ObterAsync(99, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        #endregion

        #region EnviarAsync

        [Fact]
        public async Task EnviarAsync_Existente_DeveRetornarOkComLink()
        {
            var link = new LinkAssinadoDTO();
            _serviceMock.Setup(s => s.EnviarAsync(10, 60, _ct)).ReturnsAsync(link);

            var result = await _sut.EnviarAsync(10, 60, _ct);

            var ok = Assert.IsType<Ok<LinkAssinadoDTO>>(result.Result);
            Assert.Same(link, ok.Value);
        }

        [Fact]
        public async Task EnviarAsync_Inexistente_DeveRetornarNotFound()
        {
            _serviceMock.Setup(s => s.EnviarAsync(99, null, _ct)).ReturnsAsync((LinkAssinadoDTO?)null);

            var result = await _sut.EnviarAsync(99, null, _ct);

            Assert.IsType<NotFound>(result.Result);
        }

        #endregion

        #region VisualizarAsync

        [Fact]
        public async Task VisualizarAsync_Existente_DeveRetornarPaginaHtml()
        {
            _serviceMock.Setup(s => s.ObterAsync(10, _ct)).ReturnsAsync(CriarDto());

            var result = await _sut.VisualizarAsync(10, _ct);

            Assert.True(result.StatusCode is null or StatusCodes.Status200OK);
            Assert.False(string.IsNullOrWhiteSpace(result.Content));
        }

        [Fact]
        public async Task VisualizarAsync_Inexistente_DeveRetornarPagina404()
        {
            _serviceMock.Setup(s => s.ObterAsync(99, _ct)).ReturnsAsync((AcompanhamentoOrdemServicoDTO?)null);

            var result = await _sut.VisualizarAsync(99, _ct);

            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(result.Content));
        }

        [Fact]
        public async Task VisualizarAsync_OperacaoInvalida_DeveRetornarPagina400()
        {
            _serviceMock.Setup(s => s.ObterAsync(10, _ct)).ThrowsAsync(new InvalidOperationException("erro"));

            var result = await _sut.VisualizarAsync(10, _ct);

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(result.Content));
        }

        #endregion
    }
}