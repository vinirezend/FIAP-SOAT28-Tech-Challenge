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
        public IActionResult SignIn([FromBody] UsuarioCredenciaisDTO usuarioCredenciais)
        {
            if (usuarioCredenciais == null) return BadRequest("Request inválida");

            var token = _tokenService.ValidarCredenciais(usuarioCredenciais);

            if (token == null)
                return Unauthorized();

            return Ok(token.Token);
        }
    }
}
