using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.DTOs;
using Oficina.Api.DTO.Insumo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Oficina.Api.Controllers
{

    /// <summary>
    /// Cadastro de insumos (peças e materiais) e controle de estoque e reserva.
    /// </summary>
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

        /// <summary>
        /// Obtém um insumo pelo id.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorIdAsync(int id)
        {
            var insumo = await _insumoService.ObterPorIdAsync(id);
            if (insumo == null)
                return NotFound();

            return Ok(TransformaParaResponse(insumo));
        }
        /// <summary>
        /// Lista todos os insumos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObterTodosAsync()
        {
            var insumos = await _insumoService.ObterTodosAsync();
            return Ok(insumos.Select(TransformaParaResponse));
        }
        /// <summary>
        /// Pesquisa insumos pelo termo informado.
        /// </summary>
        [HttpGet("pesquisar/{termo}")]
        public async Task<IActionResult> PesquisarAsync(string termo)
        {
            var insumos = await _insumoService.PesquisarAsync(termo);
            return Ok(insumos.Select(TransformaParaResponse));
        }
        /// <summary>
        /// Cadastra um novo insumo com a quantidade inicial em estoque.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AdicionarAsync([FromBody] InsumoCreateRequest request)
        {
            var insumo = TransformaParaDTO(request);
            var insumoCriado = await _insumoService.AdicionarAsync(insumo);
            return Created($"/api/insumo/{insumoCriado.IdInsumo}", TransformaParaResponse(insumoCriado));
        }
        /// <summary>
        /// Atualiza os dados de um insumo, incluindo quantidades em estoque e reservada.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarAsync(int id, [FromBody] InsumoUpdateRequest request)
        {
            var insumo = TransformaParaDTO(id, request);
            var insumoAtualizado = await _insumoService.AtualizarAsync(insumo);
            if (insumoAtualizado == null)
                return NotFound();
            return Ok(TransformaParaResponse(insumoAtualizado));
        }

        /// <summary>
        /// Remove um insumo.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletarAsync(int id)
        {
            var insumo = await _insumoService.ObterPorIdAsync(id);
            if (insumo == null)
                return NotFound();

            await _insumoService.RemoverAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Dá entrada da quantidade informada no estoque do insumo.
        /// </summary>
        [HttpPatch("{idInsumo:int}/AdicionarEstoque/{quantidade:int}")]
        public async Task<IActionResult> AdicionarEstoque(int idInsumo, int quantidade)
        {
            var retorno = await _insumoService.AdicionarEstoqueAsync(idInsumo, quantidade);

            return Ok(TransformaParaResponse(retorno));
        }

        /// <summary>
        /// Dá baixa da quantidade informada no estoque do insumo.
        /// </summary>
        [HttpPatch("{idInsumo:int}/RetirarEstoque/{quantidade:int}")]
        public async Task<IActionResult> RetirarEstoque(int idInsumo, int quantidade)
        {
            var retorno = await _insumoService.RetirarEstoqueAsync(idInsumo, quantidade);

            return Ok(TransformaParaResponse(retorno));
        }

        /// <summary>
        /// Reserva a quantidade informada do insumo.
        /// </summary>
        [HttpPatch("{idInsumo:int}/AdicionarReserva/{quantidade:int}")]
        public async Task<IActionResult> AdicionarReserva(int idInsumo, int quantidade)
        {
            var retorno = await _insumoService.AdicionarReservaAsync(idInsumo, quantidade);

            return Ok(TransformaParaResponse(retorno));
        }

        /// <summary>
        /// Libera a quantidade informada da reserva do insumo.
        /// </summary>
        [HttpPatch("{idInsumo:int}/RetirarReserva/{quantidade:int}")]
        public async Task<IActionResult> RetirarReserva(int idInsumo, int quantidade)
        {
            var retorno = await _insumoService.RetirarReservaAsync(idInsumo, quantidade);

            return Ok(TransformaParaResponse(retorno));
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

        private static InsumoQuantidadeResponse TransformaParaResponse(InsumoQuantidadeDTO dto)
        {
            return new InsumoQuantidadeResponse()
            {
                Nome = dto.Nome,
                QuantidadeEstoque = dto.QuantidadeEstoque,
                QuantidadeReservada = dto.QuantidadeReservada
            };
        }
    }
}
