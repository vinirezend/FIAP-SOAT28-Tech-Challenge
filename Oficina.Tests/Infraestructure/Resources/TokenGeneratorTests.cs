
using Microsoft.IdentityModel.Tokens;
using Moq;
using Oficina.Application.DTOs;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.Models;
using Oficina.Infrastructure.Configurations;
using Oficina.Infrastructure.Resources;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Oficina.Tests.Infraestructure.Resources;

public class TokenGeneratorTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly TokenGenerator _tokenGenerator;
    private readonly TokenConfiguration _configuration;

    public TokenGeneratorTests()
    {
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();

        _configuration = new TokenConfiguration
        {
            Secret = "chave-de-teste-com-tamanho-suficiente-123456",
            Issuer = "Oficina.Tests",
            Audience = "Oficina.Tests.Client",
            Minutes = 60
        };

        _tokenGenerator = new TokenGenerator(
            _configuration,
            _usuarioRepositoryMock.Object,
            new PasswordHasher());
    }

    private static readonly string SenhaHash = new PasswordHasher().Gerar("senha123");

    private static UsuarioAutenticacao CriarUsuarioValido()
    {
        return new UsuarioAutenticacao
        {
            IdUsuario = 1,
            Login = "usuario@teste.com",
            Senha = SenhaHash,
            UsuarioAtivo = true,
            IdColaborador = 10,
            Cpf = "12345678900",
            Email = "usuario@empresa.com",
            IdDepartamento = 2,
            ColaboradorAtivo = true
        };
    }

    private static UsuarioCredenciaisDTO CriarCredenciais()
    {
        return new UsuarioCredenciaisDTO
        {
            Login = "usuario@teste.com",
            Senha = "senha123"
        };
    }

    private void ConfigurarUsuarioEncontrado(
        UsuarioAutenticacao usuario,
        UsuarioCredenciaisDTO credenciais)
    {
        _usuarioRepositoryMock
            .Setup(repository => repository.ObterPorLoginAsync(credenciais.Login))
            .ReturnsAsync(usuario);
    }

    [Fact]
    public async Task ValidarCredenciais_ComCredenciaisValidas_DeveGerarToken()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var credenciais = CriarCredenciais();

        ConfigurarUsuarioEncontrado(usuario, credenciais);

        // Act
        var resultado = await _tokenGenerator.ValidarCredenciais(
            credenciais);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));

        _usuarioRepositoryMock.Verify(
            repository => repository.ObterPorLoginAsync(credenciais.Login),
            Times.Once);
    }

    [Fact]
    public async Task ValidarCredenciais_UsuarioNaoEncontrado_DeveRetornarNull()
    {
        // Arrange
        var credenciais = CriarCredenciais();

        _usuarioRepositoryMock
            .Setup(repository => repository.ObterPorLoginAsync(credenciais.Login))
            .ReturnsAsync((UsuarioAutenticacao?)null);

        // Act
        var resultado = await _tokenGenerator.ValidarCredenciais(
            credenciais);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ValidarCredenciais_SenhaIncorreta_DeveRetornarNull()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var credenciais = CriarCredenciais();

        ConfigurarUsuarioEncontrado(usuario, credenciais);
        credenciais.Senha = "senha-errada";

        // Act
        var resultado = await _tokenGenerator.ValidarCredenciais(
            credenciais);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ValidarCredenciais_DeveConterClaimsEsperadas()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var credenciais = CriarCredenciais();

        ConfigurarUsuarioEncontrado(usuario, credenciais);

        // Act
        var resultado = await _tokenGenerator.ValidarCredenciais(
            credenciais);

        // Assert
        Assert.NotNull(resultado);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(resultado.Token);

        Assert.Equal(_configuration.Issuer, jwt.Issuer);
        Assert.Contains(_configuration.Audience, jwt.Audiences);

        Assert.Contains(
            jwt.Claims,
            claim => claim.Type == JwtRegisteredClaimNames.UniqueName
                && claim.Value == usuario.Login);

        Assert.Contains(
            jwt.Claims,
            claim => claim.Type == JwtRegisteredClaimNames.Jti);

        Assert.Contains(
            jwt.Claims,
            claim => claim.Type == ClaimTypes.NameIdentifier
                && claim.Value == usuario.IdUsuario.ToString());

        Assert.Contains(
            jwt.Claims,
            claim => claim.Type == ClaimTypes.Email
                && claim.Value == usuario.Email);

        Assert.Contains(
            jwt.Claims,
            claim => claim.Type == "cpf"
                && claim.Value == usuario.Cpf);

        Assert.Contains(
            jwt.Claims,
            claim => claim.Type == "id_colaborador"
                && claim.Value == usuario.IdColaborador.ToString());

        Assert.Contains(
            jwt.Claims,
            claim => claim.Type == "id_departamento"
                && claim.Value == usuario.IdDepartamento.ToString());
    }

    [Fact]
    public async Task ValidarCredenciais_DeveGerarTokenComAssinaturaValida()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var credenciais = CriarCredenciais();

        ConfigurarUsuarioEncontrado(usuario, credenciais);

        // Act
        var resultado = await _tokenGenerator.ValidarCredenciais(
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

        Assert.True(principal.Identity?.IsAuthenticated);
        Assert.IsType<JwtSecurityToken>(tokenValidado);

        var jwt = (JwtSecurityToken)tokenValidado;
        var agora = DateTime.UtcNow;

        Assert.True(jwt.ValidTo > agora);
        Assert.True(
            jwt.ValidTo <= agora.AddMinutes(_configuration.Minutes)
                .AddSeconds(5));
    }


    [Fact]
    public async Task ValidarCredenciais_ColaboradorInativo_DeveRetornarNull()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        usuario.ColaboradorAtivo = false;

        var credenciais = CriarCredenciais();

        ConfigurarUsuarioEncontrado(usuario, credenciais);

        // Act
        var resultado = await _tokenGenerator.ValidarCredenciais(
            credenciais);

        // Assert
        Assert.Null(resultado);
    }


    [Fact]
    public async Task ValidarCredenciais_ColaboradorComStatusNulo_DeveRetornarNull()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        usuario.ColaboradorAtivo = null;

        var credenciais = CriarCredenciais();

        ConfigurarUsuarioEncontrado(usuario, credenciais);

        // Act
        var resultado = await _tokenGenerator.ValidarCredenciais(
            credenciais);

        // Assert
        Assert.Null(resultado);
    }


    [Fact]
    public async Task ValidarCredenciais_SemColaboradorVinculado_DeveGerarToken()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        usuario.IdColaborador = null;
        usuario.ColaboradorAtivo = false;

        var credenciais = CriarCredenciais();

        ConfigurarUsuarioEncontrado(usuario, credenciais);

        // Act
        var resultado = await _tokenGenerator.ValidarCredenciais(
            credenciais);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
    }

}
