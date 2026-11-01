using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Api.DTO.Status;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Tests.Api.Controllers
{
    public class StatusControllerTests
    {
        private readonly Mock<IStatusService> _statusService = new();
        private readonly StatusController _sut;

        public StatusControllerTests()
        {
            _sut = new StatusController(_statusService.Object);
        }

        // ---------- ObterTodosAsync ----------

        [Fact]
        public async Task ObterTodosAsync_ExistemStatus_RetornaOkComTodosMapeados()
        {
            var token = new CancellationTokenSource().Token;
            _statusService
                .Setup(s => s.ObterTodosAsync(token))
                .ReturnsAsync(new List<StatusDTO>
                {
                new() { IdStatus = 1, Nome = "Recebida" },
                new() { IdStatus = 2, Nome = "Em execução" },
                new() { IdStatus = 3, Nome = "Finalizada" }
                });

            var resultado = await _sut.ObterTodosAsync(token);

            var lista = resultado.Value!.ToList();
            Assert.Equal(3, lista.Count);
            Assert.Equal(1, lista[0].IdStatus);
            Assert.Equal("Recebida", lista[0].Nome);
            Assert.Equal(2, lista[1].IdStatus);
            Assert.Equal("Em execução", lista[1].Nome);
            Assert.Equal(3, lista[2].IdStatus);
            Assert.Equal("Finalizada", lista[2].Nome);
            _statusService.Verify(s => s.ObterTodosAsync(token), Times.Once);
        }

        [Fact]
        public async Task ObterTodosAsync_NaoExistemStatus_RetornaOkComListaVazia()
        {
            _statusService
                .Setup(s => s.ObterTodosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StatusDTO>());

            var resultado = await _sut.ObterTodosAsync(CancellationToken.None);

            Assert.Equal(StatusCodes.Status200OK, resultado.StatusCode);
            Assert.Empty(resultado.Value!);
        }

        // ---------- ObterPorIdAsync ----------

        [Fact]
        public async Task ObterPorIdAsync_StatusExiste_RetornaOkComStatusMapeado()
        {
            var token = new CancellationTokenSource().Token;
            _statusService
                .Setup(s => s.ObterPorIdAsync(2, token))
                .ReturnsAsync(new StatusDTO { IdStatus = 2, Nome = "Em execução" });

            var resultado = await _sut.ObterPorIdAsync(2, token);

            var ok = Assert.IsType<Ok<StatusResponse>>(resultado.Result);
            Assert.Equal(2, ok.Value!.IdStatus);
            Assert.Equal("Em execução", ok.Value.Nome);
            _statusService.Verify(s => s.ObterPorIdAsync(2, token), Times.Once);
        }

        [Fact]
        public async Task ObterPorIdAsync_StatusNaoExiste_RetornaNotFoundComMensagem()
        {
            _statusService
                .Setup(s => s.ObterPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StatusDTO?)null);

            var resultado = await _sut.ObterPorIdAsync(999, CancellationToken.None);

            var naoEncontrado = Assert.IsType<NotFound<string>>(resultado.Result);
            Assert.Equal("status não encontrado.", naoEncontrado.Value);
        }

        // ---------- Metadados do controller ----------

        [Fact]
        public void Controller_PossuiAtributosAuthorizeEApiController()
        {
            var tipo = typeof(StatusController);

            Assert.NotNull(tipo.GetCustomAttribute<AuthorizeAttribute>());
            Assert.NotNull(tipo.GetCustomAttribute<ApiControllerAttribute>());
        }
    }
}