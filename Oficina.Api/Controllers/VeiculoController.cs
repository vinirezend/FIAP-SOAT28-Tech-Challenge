using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.DTOs;

namespace Oficina.Api.Controllers
{
            [ApiController]
        [Route("api/[controller]")]
    public class VeiculoController : ControllerBase
    {
        private readonly IVeiculoService _veiculoService;

        public VeiculoController(IVeiculoService veiculoService)
        {
            _veiculoService = veiculoService;
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodosAsync()
        {
            var veiculos = await _veiculoService.ObterTodosAsync();
            return Ok(veiculos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorIdAsync(int id)
        {
            var veiculo = await _veiculoService.ObterPorIdAsync(id);
            if (veiculo is null) return NotFound();
            return Ok(veiculo);
        }

        [HttpGet("placa/{placa}")]
        public async Task<IActionResult> ObterPorPlacaAsync(string placa)
        {
            var veiculo = await _veiculoService.ObterPorPlacaAsync(placa);
            if (veiculo is null) return NotFound();
            return Ok(veiculo);
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarAsync(VeiculoDTO veiculo)
        {
            var veiculoAdicionado = await _veiculoService.AdicionarAsync(veiculo);
            return Created($"/api/veiculo/{veiculoAdicionado.IdVeiculo}", veiculoAdicionado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarAsync(int id, VeiculoDTO veiculo)
        {
            if (id != veiculo.IdVeiculo) return BadRequest();
            var veiculoAtualizado = await _veiculoService.AtualizarAsync(veiculo);
            if (veiculoAtualizado is null) return NotFound();
            return Ok(veiculoAtualizado);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> RemoverAsync(int id)
        {   var veiculo = await _veiculoService.ObterPorIdAsync(id);
            if (veiculo is null) return NotFound();
            await _veiculoService.RemoverAsync(id);
            return NoContent();
        }
    }
}