using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.DTOs;
namespace Oficina.Api.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class ServicoController : ControllerBase
    {
        private readonly IServicoService _servicoService;

        public ServicoController(IServicoService servicoService)
        {
            _servicoService = servicoService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> ObterPorIdAsync(int id){
            var servico = await _servicoService.ObterPorIdAsync(id);
            if (servico == null)
                return NotFound();

            return Ok(servico);
        }
        [HttpGet]
        public async Task<ActionResult> ObterTodosAsync()
        {
            var servicos = await _servicoService.ObterTodosAsync();
            return Ok(servicos);
        }
        [HttpGet("pesquisar/{termo}")]
        public async Task<ActionResult> PesquisarAsync(string termo)
        {
            var servicos = await _servicoService.PesquisarAsync(termo);
            return Ok(servicos);
        }
        [HttpPost]
        public async Task<ActionResult> AdicionarAsync([FromBody] ServicoDTO servico)
        {
            var servicoAdicionado = await _servicoService.AdicionarAsync(servico);
            return CreatedAtAction(nameof(ObterPorIdAsync), new { id = servicoAdicionado.IdServico }, servicoAdicionado);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> AtualizarAsync(int id, [FromBody] ServicoDTO servico)
        {
            servico.IdServico = id;
            var servicoAtualizado = await _servicoService.AtualizarAsync(servico);
            if (servicoAtualizado == null)
                return NotFound();

            return Ok(servicoAtualizado);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletarAsync(int id)
        {
            var servico = await _servicoService.ObterPorIdAsync(id);
            if (servico == null)
                return NotFound();

            await _servicoService.RemoverAsync(id);
            return NoContent();
        }
    }
}