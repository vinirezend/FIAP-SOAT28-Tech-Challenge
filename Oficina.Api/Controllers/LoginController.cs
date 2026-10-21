using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private ITokenService _tokenService;

        public LoginController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

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
