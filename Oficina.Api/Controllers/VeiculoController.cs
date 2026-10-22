using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.DTOs;
using Oficina.Api.DTO.Veiculo;

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
        public async Task<IActionResult> AdicionarAsync([FromBody] VeiculoRequest veiculoRequest)
        {
            var veiculo = TransformaParaDTO(veiculoRequest);
            var veiculoAdicionado = await _veiculoService.AdicionarAsync(veiculo);
            return Created($"/api/veiculo/{veiculoAdicionado.IdVeiculo}", TransformaParaResponse(veiculoAdicionado));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarAsync(int id, [FromBody] VeiculoRequest veiculoRequest)
        {
            var veiculo = TransformaParaDTO(veiculoRequest);
            if (id != veiculo.IdVeiculo) return BadRequest();
            var veiculoAtualizado = await _veiculoService.AtualizarAsync(veiculo);
            if (veiculoAtualizado is null) return NotFound();
            return Ok(TransformaParaResponse(veiculoAtualizado));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> RemoverAsync(int id)
        {   var veiculo = await _veiculoService.ObterPorIdAsync(id);
            if (veiculo is null) return NotFound();
            await _veiculoService.RemoverAsync(id);
            return NoContent();
        }

        private VeiculoDTO TransformaParaDTO(VeiculoRequest request)
        {
            return new VeiculoDTO
            {
                Placa = request.Placa,
                Modelo = request.Modelo,
                Marca = request.Marca,
                Ano = request.Ano,
                IdCliente = request.IdCliente,
                IdTipoVeiculo = request.IdTipoVeiculo
            };
        }
        private VeiculoResponse TransformaParaResponse(VeiculoDTO veiculo)
        {
            return new VeiculoResponse
            {
                IdVeiculo = veiculo.IdVeiculo,
                Placa = veiculo.Placa,
                Modelo = veiculo.Modelo,
                Marca = veiculo.Marca,
                Ano = veiculo.Ano,
                IdCliente = veiculo.IdCliente,
                IdTipoVeiculo = veiculo.IdTipoVeiculo
            };
        }
    }
}