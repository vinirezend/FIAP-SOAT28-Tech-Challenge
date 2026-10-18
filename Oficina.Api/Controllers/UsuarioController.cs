using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsuarioController : Controller
    {
        private IUsuarioRepository _usuarioRepository;

        public UsuarioController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Obter(int id)
        {
            var retorno = _usuarioRepository.Obter(id).Result;

            if (retorno == null)
                return BadRequest("Usuário não encontrado.");

            return Ok(retorno);
        }
    }
}
