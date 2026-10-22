using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.DTOs;

namespace Oficina.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class InsumoController : ControllerBase
    {
        private readonly IInsumoService _insumoService;

        public InsumoController(IInsumoService insumoService)
        {
            _insumoService = insumoService;
        }
       
       [HttpGet("{id}")]
        public async Task<ActionResult> ObterPorIdAsync(int id)
        {
            var insumo = await _insumoService.ObterPorIdAsync(id);
            if (insumo == null)
                return NotFound();

            return Ok(insumo);
        }
        [HttpGet]
        public async Task<IActionResult> ObterTodosAsync()
        {
            var insumos = await _insumoService.ObterTodosAsync();
            return Ok(insumos);
        }
        [HttpGet("pesquisar/{termo}")]
        public async Task<IActionResult> PesquisarAsync(string termo)
        {
            var insumos = await _insumoService.PesquisarAsync(termo);
            return Ok(insumos);
        }
        [HttpPost]
        public async Task<IActionResult> AdicionarAsync([FromBody] InsumoDTO insumoDTO)
        {
            var insumoCriado = await _insumoService.AdicionarAsync(insumoDTO);
            return CreatedAtAction(nameof(ObterPorIdAsync), new { id = insumoCriado.IdInsumo }, insumoCriado);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarAsync(int id, [FromBody] InsumoDTO insumo)
        {
                insumo.IdInsumo = id;
                var insumoAtualizado = await _insumoService.AtualizarAsync(insumo);
                if (insumoAtualizado == null)
                    return NotFound();
                return Ok(insumoAtualizado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarAsync(int id)
        {
            var insumo = await _insumoService.ObterPorIdAsync(id);
            if (insumo == null)
                return NotFound();

            await _insumoService.RemoverAsync(id);
            return NoContent();
        }
    }
}