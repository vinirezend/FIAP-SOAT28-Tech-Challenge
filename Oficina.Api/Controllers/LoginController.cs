using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Api.Controllers
{
    /// <summary>
    /// Autenticação de usuários da oficina.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private ITokenGenerator _tokenService;

        public LoginController(ITokenGenerator tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Valida login e senha e retorna o token JWT para usar no header Authorization (Bearer).
        /// </summary>
        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> SignIn([FromBody] UsuarioCredenciaisDTO usuarioCredenciais)
        {
            if (usuarioCredenciais == null) return BadRequest("Request inválida");

            var token = await _tokenService.ValidarCredenciais(usuarioCredenciais);

            if (token == null)
                return Unauthorized("Login ou senha incorretos.");

            return Ok(token.Token);
        }
    }
}
