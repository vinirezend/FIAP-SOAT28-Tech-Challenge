using Microsoft.AspNetCore.Http;
using Oficina.Infrastructure.Resources;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Oficina.Tests.Infraestructure.Resources
{
    public class CurrentUserTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly CurrentUserService _service;

        public CurrentUserTests()
        {
            _httpContext = new DefaultHttpContext();

            var accessor = new HttpContextAccessor
            {
                HttpContext = _httpContext
            };

            _service = new CurrentUserService(accessor);
        }

        private void ConfigurarUsuarioAutenticado(
            params Claim[] claims)
        {
            _httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, "TestAuthentication"));
        }

        [Fact]
        public void Autenticado_UsuarioAutenticado_DeveRetornarTrue()
        {
            ConfigurarUsuarioAutenticado(
                new Claim(ClaimTypes.NameIdentifier, "1"));

            Assert.True(_service.Autenticado);
        }

        [Fact]
        public void Autenticado_SemUsuarioAutenticado_DeveRetornarFalse()
        {
            _httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity());

            Assert.False(_service.Autenticado);
        }

        [Fact]
        public void IdUsuario_ClaimValida_DeveRetornarId()
        {
            ConfigurarUsuarioAutenticado(
                new Claim(ClaimTypes.NameIdentifier, "10"));

            Assert.Equal(10, _service.IdUsuario);
        }

        [Fact]
        public void IdUsuario_ClaimAusente_DeveRetornarNull()
        {
            ConfigurarUsuarioAutenticado(
                new Claim(ClaimTypes.Email, "teste@empresa.com"));

            Assert.Null(_service.IdUsuario);
        }

        [Fact]
        public void IdUsuario_ClaimInvalida_DeveRetornarNull()
        {
            ConfigurarUsuarioAutenticado(
                new Claim(ClaimTypes.NameIdentifier, "abc"));

            Assert.Null(_service.IdUsuario);
        }

        [Fact]
        public void IdColaborador_ClaimValida_DeveRetornarId()
        {
            ConfigurarUsuarioAutenticado(
                new Claim("id_colaborador", "7"));

            Assert.Equal(7, _service.IdColaborador);
        }

        [Fact]
        public void IdColaborador_ClaimAusente_DeveRetornarNull()
        {
            ConfigurarUsuarioAutenticado(
                new Claim(ClaimTypes.NameIdentifier, "1"));

            Assert.Null(_service.IdColaborador);
        }

        [Fact]
        public void IdDepartamento_ClaimValida_DeveRetornarId()
        {
            ConfigurarUsuarioAutenticado(
                new Claim("id_departamento", "5"));

            Assert.Equal(5, _service.IdDepartamento);
        }

        [Fact]
        public void IdDepartamento_ClaimAusente_DeveRetornarNull()
        {
            ConfigurarUsuarioAutenticado(
                new Claim(ClaimTypes.NameIdentifier, "1"));

            Assert.Null(_service.IdDepartamento);
        }

        [Fact]
        public void IdDepartamento_ClaimInvalida_DeveRetornarNull()
        {
            ConfigurarUsuarioAutenticado(
                new Claim("id_departamento", "departamento"));

            Assert.Null(_service.IdDepartamento);
        }

        [Fact]
        public void Cpf_ClaimExistente_DeveRetornarValor()
        {
            ConfigurarUsuarioAutenticado(
                new Claim("cpf", "52998224725"));

            Assert.Equal("52998224725", _service.Cpf);
        }

        [Fact]
        public void Cpf_ClaimAusente_DeveRetornarNull()
        {
            ConfigurarUsuarioAutenticado();

            Assert.Null(_service.Cpf);
        }

        [Fact]
        public void Email_ClaimExistente_DeveRetornarValor()
        {
            ConfigurarUsuarioAutenticado(
                new Claim(ClaimTypes.Email, "teste@empresa.com"));

            Assert.Equal("teste@empresa.com", _service.Email);
        }

        [Fact]
        public void Email_ClaimAusente_DeveRetornarNull()
        {
            ConfigurarUsuarioAutenticado();

            Assert.Null(_service.Email);
        }

        [Fact]
        public void Login_ClaimExistente_DeveRetornarValor()
        {
            ConfigurarUsuarioAutenticado(
                new Claim(
                    JwtRegisteredClaimNames.UniqueName,
                    "usuario@teste.com"));

            Assert.Equal("usuario@teste.com", _service.Login);
        }

        [Fact]
        public void Login_ClaimAusente_DeveRetornarNull()
        {
            ConfigurarUsuarioAutenticado();

            Assert.Null(_service.Login);
        }

        [Fact]
        public void UsuarioAusente_DeveRetornarValoresPadrao()
        {
            _httpContext.User = null!;

            Assert.False(_service.Autenticado);
            Assert.Null(_service.IdUsuario);
            Assert.Null(_service.IdDepartamento);
            Assert.Null(_service.Cpf);
            Assert.Null(_service.Email);
            Assert.Null(_service.Login);
        }
    }
}
