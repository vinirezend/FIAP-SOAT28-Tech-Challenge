using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.DTOs;
using Oficina.Api.DTO.Insumo;
using Microsoft.AspNetCore.Authorization;

namespace Oficina.Api.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class InsumoController : ControllerBase
    {
        private readonly IInsumoService _insumoService;

        public InsumoController(IInsumoService insumoService)
        {
            _insumoService = insumoService;
        }

       [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorIdAsync(int id)
        {
            var insumo = await _insumoService.ObterPorIdAsync(id);
            if (insumo == null)
                return NotFound();

            return Ok(TransformaParaResponse(insumo));
        }
        [HttpGet]
        public async Task<IActionResult> ObterTodosAsync()
        {
            var insumos = await _insumoService.ObterTodosAsync();
            return Ok(insumos.Select(TransformaParaResponse));
        }
        [HttpGet("pesquisar/{termo}")]
        public async Task<IActionResult> PesquisarAsync(string termo)
        {
            var insumos = await _insumoService.PesquisarAsync(termo);
            return Ok(insumos.Select(TransformaParaResponse));
        }
        [HttpPost]
        public async Task<IActionResult> AdicionarAsync([FromBody] InsumoCreateRequest request)
        {
            var insumo = TransformaParaDTO(request);
            var insumoCriado = await _insumoService.AdicionarAsync(insumo);
            return Created($"/api/insumo/{insumoCriado.IdInsumo}", TransformaParaResponse(insumoCriado));
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarAsync(int id, [FromBody] InsumoUpdateRequest request)
        {
            var insumo = TransformaParaDTO(id, request);
            var insumoAtualizado = await _insumoService.AtualizarAsync(insumo);
            if (insumoAtualizado == null)
                return NotFound();
            return Ok(TransformaParaResponse(insumoAtualizado));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletarAsync(int id)
        {
            var insumo = await _insumoService.ObterPorIdAsync(id);
            if (insumo == null)
                return NotFound();

            await _insumoService.RemoverAsync(id);
            return NoContent();
        }

        private static InsumoDTO TransformaParaDTO(InsumoCreateRequest request)
        {
            return new InsumoDTO
            {
                Nome = request.Nome,
                Descricao = request.Descricao,
                ValorUnitario = request.ValorUnitario,
                QuantidadeEstoque = request.QuantidadeEstoque
            };
        }
        private static InsumoDTO TransformaParaDTO(int id, InsumoUpdateRequest request)
        {
            return new InsumoDTO
            {
                IdInsumo = id,
                Nome = request.Nome,
                Descricao = request.Descricao,
                ValorUnitario = request.ValorUnitario,
                Ativo = request.Ativo,
                QuantidadeEstoque = request.QuantidadeEstoque,
                QuantidadeReservada = request.QuantidadeReservada
            };
        }
        private static InsumoResponse TransformaParaResponse(InsumoDTO dto)
        {
            return new InsumoResponse
            {
                IdInsumo = dto.IdInsumo,
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                ValorUnitario = dto.ValorUnitario,
                Ativo = dto.Ativo,
                QuantidadeEstoque = dto.QuantidadeEstoque,
                QuantidadeReservada = dto.QuantidadeReservada
            };
        }
    }
}
