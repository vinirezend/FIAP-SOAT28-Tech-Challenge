using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Api.DTO.Usuario;
using Oficina.Domain.Interfaces.Repositories;
using UsuarioEntity = Oficina.Domain.Entities.Usuario;

namespace Oficina.Tests.Api.Controllers
{
    public class UsuarioControllerTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
        private readonly UsuarioController _sut;

        public UsuarioControllerTests()
        {
            _sut = new UsuarioController(_usuarioRepository.Object);
        }

        // Se a entidade não tiver setters públicos / construtor sem parâmetros,
        // adapte somente este helper.
        private static UsuarioEntity CriarUsuario(int id, string login, bool ativo) => new()
        {
            IdUsuario = id,
            Login = login,
            Ativo = ativo
        };

        [Fact]
        public async Task ObterAsync_UsuarioExiste_RetornaOkComUsuarioMapeado()
        {
            var token = new CancellationTokenSource().Token;
            _usuarioRepository
                .Setup(r => r.ObterAsync(10, token))
                .ReturnsAsync(CriarUsuario(10, "joao.silva", ativo: true));

            var resultado = await _sut.ObterAsync(10, token);

            var ok = Assert.IsType<Ok<UsuarioResponse>>(resultado.Result);
            Assert.Equal(10, ok.Value!.IdUsuario);
            Assert.Equal("joao.silva", ok.Value.Login);
            Assert.True(ok.Value.Ativo);
            _usuarioRepository.Verify(r => r.ObterAsync(10, token), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ObterAsync_UsuarioExiste_PreservaFlagAtivo(bool ativo)
        {
            _usuarioRepository
                .Setup(r => r.ObterAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CriarUsuario(5, "maria", ativo));

            var resultado = await _sut.ObterAsync(5, CancellationToken.None);

            var ok = Assert.IsType<Ok<UsuarioResponse>>(resultado.Result);
            Assert.Equal(ativo, ok.Value!.Ativo);
        }

        [Fact]
        public async Task ObterAsync_UsuarioNaoExiste_RetornaNotFound()
        {
            _usuarioRepository
                .Setup(r => r.ObterAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UsuarioEntity?)null);

            var resultado = await _sut.ObterAsync(999, CancellationToken.None);

            Assert.IsType<NotFound>(resultado.Result);
        }

        [Fact]
        public void Controller_PossuiAtributosAuthorizeEApiController()
        {
            var tipo = typeof(UsuarioController);

            Assert.NotNull(tipo.GetCustomAttribute<AuthorizeAttribute>());
            Assert.NotNull(tipo.GetCustomAttribute<ApiControllerAttribute>());
        }
    }
}