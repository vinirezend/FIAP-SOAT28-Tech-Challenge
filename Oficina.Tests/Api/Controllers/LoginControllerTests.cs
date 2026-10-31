using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Oficina.Api.Controllers;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;

namespace Oficina.Tests.API.Controllers;

public class LoginControllerTests
{
    private readonly Mock<ITokenGenerator> _tokenServiceMock = new();
    private readonly LoginController _controller;

    public LoginControllerTests()
    {
        _controller = new LoginController(_tokenServiceMock.Object)
        {
            // Problem(...) usa o ProblemDetailsFactory do HttpContext; sem ele cai no padrão do MVC
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task SignIn_ComCredenciaisInvalidas_DeveRetornarUnauthorizedComProblemDetails()
    {
        // Arrange
        var credenciais = new UsuarioCredenciaisDTO
        {
            Login = "usuario@teste.com",
            Senha = "senhaIncorreta"
        };

        _tokenServiceMock
            .Setup(service => service.ValidarCredenciais(credenciais))
            .ReturnsAsync((TokenDTO?)null);

        // Act
        var resultado = await _controller.SignIn(credenciais);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(resultado.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);

        var problema = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Login ou senha incorretos.", problema.Detail);

        _tokenServiceMock.Verify(service => service.ValidarCredenciais(credenciais), Times.Once);
    }

    [Fact]
    public async Task SignIn_ComCredenciaisValidas_DeveRetornarOkComToken()
    {
        // Arrange
        var credenciais = new UsuarioCredenciaisDTO
        {
            Login = "usuario@teste.com",
            Senha = "senha123"
        };

        const string tokenEsperado = "jwt-de-teste";

        _tokenServiceMock
            .Setup(service => service.ValidarCredenciais(credenciais))
            .ReturnsAsync(new TokenDTO(true, "2026-10-21T10:00:00Z", "2026-10-21T11:00:00Z", tokenEsperado));

        // Act
        var resultado = await _controller.SignIn(credenciais);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Equal(tokenEsperado, ok.Value);

        _tokenServiceMock.Verify(service => service.ValidarCredenciais(credenciais), Times.Once);
    }
}
