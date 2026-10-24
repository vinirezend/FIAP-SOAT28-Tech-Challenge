
using Microsoft.AspNetCore.Mvc;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;

namespace Oficina.Tests.API.Controllers
{
    public class LoginControllerTests
    {
        private readonly Mock<ITokenGenerator> _tokenServiceMock;
        private readonly LoginController _controller;

        public LoginControllerTests()
        {
            _tokenServiceMock = new Mock<ITokenGenerator>();

            _controller = new LoginController(
                _tokenServiceMock.Object);
        }

        [Fact]
        public async Task SignIn_ComCredenciaisNulas_DeveRetornarBadRequest()
        {
            // Arrange
            UsuarioCredenciaisDTO credenciais = null;

            // Act
            var resultado = await _controller.SignIn(credenciais);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(
                resultado);

            Assert.Equal("Request inválida", badRequest.Value);

            _tokenServiceMock.Verify(
                service => service.ValidarCredenciais(
                    It.IsAny<UsuarioCredenciaisDTO>()),
                Times.Never);
        }

        [Fact]
        public async Task SignIn_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
        {
            // Arrange
            var credenciais = new UsuarioCredenciaisDTO()
            {
                Login = "usuario@teste.com",
                Senha = "senhaIncorreta"
            };

            _tokenServiceMock
                .Setup(service => service.ValidarCredenciais(credenciais))
                .ReturnsAsync((TokenDTO)null);

            // Act
            var resultado = await _controller.SignIn(credenciais);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(
                resultado);

            Assert.Equal(
                "Login ou senha incorretos.",
                unauthorized.Value);

            _tokenServiceMock.Verify(
                service => service.ValidarCredenciais(credenciais),
                Times.Once);
        }

        [Fact]
        public async Task SignIn_ComCredenciaisValidas_DeveRetornarOkComToken()
        {
            // Arrange
            var credenciais = new UsuarioCredenciaisDTO()
            {
                Login = "usuario@teste.com",
                Senha = "senha123"
            };

            var tokenEsperado = "jwt-de-teste";

            var tokenDTO = new TokenDTO(
                true,
                "2026-10-21T10:00:00Z",
                "2026-10-21T11:00:00Z",
                tokenEsperado);

            _tokenServiceMock
                .Setup(service => service.ValidarCredenciais(credenciais))
                .ReturnsAsync(tokenDTO);

            // Act
            var resultado = await _controller.SignIn(credenciais);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(resultado);

            Assert.Equal(tokenEsperado, ok.Value);

            _tokenServiceMock.Verify(
                service => service.ValidarCredenciais(credenciais),
                Times.Once);
        }
    }
}
