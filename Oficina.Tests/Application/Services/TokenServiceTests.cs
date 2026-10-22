using Microsoft.IdentityModel.Tokens;
using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Implementations.Services;
using Oficina.Domain.Configurations;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Oficina.Tests.Application.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly TokenService _tokenService;
        private readonly TokenConfiguration _configuration;

        public TokenServiceTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            _configuration = new TokenConfiguration
            {
                Secret = "chave-de-teste-com-tamanho-suficiente-123456",
                Issuer = "Oficina.Tests",
                Audience = "Oficina.Tests.Client",
                Minutes = 60
            };

            _tokenService = new TokenService(
                _configuration,
                _usuarioRepositoryMock.Object);
        }

        [Fact]
        public async Task ValidarCredenciaisAsync_ComCredenciaisValidas_DeveGerarToken()
        {
            // Arrange
            var usuario = new Usuario
            {
                Login = "usuario@teste.com"
            };

            var credenciais = new UsuarioCredenciaisDTO()
            {
                Login = "usuario@teste.com",
                Senha = "senha123"
            };

            _usuarioRepositoryMock
                .Setup(repository => repository.ObterPorCredenciais(
                    credenciais.Login,
                    credenciais.Senha))
                .ReturnsAsync(usuario);

            // Act
            var resultado = await _tokenService.ValidarCredenciais(
                credenciais);

            // Assert
            Assert.NotNull(resultado);
            Assert.False(string.IsNullOrWhiteSpace(resultado.Token));

            _usuarioRepositoryMock.Verify(
                repository => repository.ObterPorCredenciais(
                    credenciais.Login,
                    credenciais.Senha),
                Times.Once);
        }

        [Fact]
        public async Task ValidarCredenciaisAsync_ComCredenciaisInvalidas_DeveRetornarNull()
        {
            // Arrange
            var credenciais = new UsuarioCredenciaisDTO()
            {
                Login = "inexistente@teste.com",
                Senha = "senhaIncorreta"
            };

            _usuarioRepositoryMock
                .Setup(repository => repository.ObterPorCredenciais(
                    credenciais.Login,
                    credenciais.Senha))
                .ReturnsAsync((Usuario)null);

            // Act
            var resultado = await _tokenService.ValidarCredenciais(
                credenciais);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ValidarCredenciaisAsync_ComCredenciaisValidas_DeveConterClaimsEsperadas()
        {
            // Arrange
            var usuario = new Usuario
            {
                Login = "usuario@teste.com"
            };

            var credenciais = new UsuarioCredenciaisDTO()
            {
                Login = "usuario@teste.com",
                Senha = "senha123"
            };

            _usuarioRepositoryMock
                .Setup(repository => repository.ObterPorCredenciais(
                    credenciais.Login,
                    credenciais.Senha))
                .ReturnsAsync(usuario);

            // Act
            var resultado = await _tokenService.ValidarCredenciais(
                credenciais);

            // Assert
            Assert.NotNull(resultado);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(resultado.Token);

            Assert.Equal(_configuration.Issuer, jwt.Issuer);

            Assert.Contains(
                jwt.Claims,
                claim => claim.Type == JwtRegisteredClaimNames.UniqueName
                         && claim.Value == usuario.Login);

            Assert.Contains(
                jwt.Claims,
                claim => claim.Type == JwtRegisteredClaimNames.Jti);

            Assert.Equal(
                _configuration.Audience,
                jwt.Audiences.Single());
        }

        [Fact]
        public async Task ValidarCredenciaisAsync_ComCredenciaisValidas_DeveGerarTokenComAssinaturaValida()
        {
            // Arrange
            var usuario = new Usuario
            {
                Login = "usuario@teste.com"
            };

            var credenciais = new UsuarioCredenciaisDTO()
            {
                Login = "usuario@teste.com",
                Senha = "senha123"
            };

            _usuarioRepositoryMock
                .Setup(repository => repository.ObterPorCredenciais(
                    credenciais.Login,
                    credenciais.Senha))
                .ReturnsAsync(usuario);

            // Act
            var resultado = await _tokenService.ValidarCredenciais(
                credenciais);

            // Assert
            Assert.NotNull(resultado);

            var handler = new JwtSecurityTokenHandler();

            var parametrosValidacao = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration.Issuer,

                ValidateAudience = true,
                ValidAudience = _configuration.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration.Secret)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = handler.ValidateToken(
                resultado.Token,
                parametrosValidacao,
                out var tokenValidado);

            Assert.NotNull(principal.Identity);
            Assert.True(principal.Identity.IsAuthenticated);

            Assert.IsType<JwtSecurityToken>(tokenValidado);

            var jwt = (JwtSecurityToken)tokenValidado;

            Assert.True(jwt.ValidTo > DateTime.UtcNow);
            Assert.True(
                jwt.ValidTo <= DateTime.UtcNow.AddMinutes(
                    _configuration.Minutes).AddSeconds(5));
        }
    }
}
