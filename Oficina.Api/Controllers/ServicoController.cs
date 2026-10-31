using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.DTOs;
using Oficina.Api.DTO.Servico;
using Microsoft.AspNetCore.Authorization;

namespace Oficina.Api.Controllers
{
    
    /// <summary>
    /// Catálogo de serviços oferecidos pela oficina.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ServicoController : ControllerBase
    {
        private readonly IServicoService _servicoService;

        public ServicoController(IServicoService servicoService)
        {
            _servicoService = servicoService;
        }

        /// <summary>
        /// Obtém um serviço pelo id.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorIdAsync(int id){
            var servico = await _servicoService.ObterPorIdAsync(id);
            if (servico == null)
                return NotFound();

            return Ok(TransformaParaResponse(servico));
        }
        /// <summary>
        /// Lista todos os serviços.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObterTodosAsync()
        {
            var servicos = await _servicoService.ObterTodosAsync();
            return Ok(servicos.Select(s => TransformaParaResponse(s)));
        }
        /// <summary>
        /// Pesquisa serviços pelo termo informado.
        /// </summary>
        [HttpGet("pesquisar/{termo}")]
        public async Task<IActionResult> PesquisarAsync(string termo)
        {
            var servicos = await _servicoService.PesquisarAsync(termo);
            return Ok(servicos.Select(s => TransformaParaResponse(s)));
        }
        /// <summary>
        /// Cadastra um novo serviço, já ativo.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AdicionarAsync([FromBody] ServicoCreateRequest servico)
        {
            var servicoAdicionado = await _servicoService.AdicionarAsync(TransformaParaDTO(servico));
            return Created($"/api/servico/{servicoAdicionado.IdServico}", TransformaParaResponse(servicoAdicionado));
        }
        /// <summary>
        /// Atualiza os dados de um serviço, incluindo se está ativo.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarAsync(int id, [FromBody] ServicoUpdateRequest servico)
        {
            var servicoAtualizado = await _servicoService.AtualizarAsync(TransformaParaDTO(id, servico));
            if (servicoAtualizado == null)
                return NotFound();

            return Ok(TransformaParaResponse(servicoAtualizado));
        }
        /// <summary>
        /// Remove um serviço.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletarAsync(int id)
        {
            var servico = await _servicoService.ObterPorIdAsync(id);
            if (servico == null)
                return NotFound();

            await _servicoService.RemoverAsync(id);
            return NoContent();
        }

        private ServicoDTO TransformaParaDTO(ServicoCreateRequest request)
        {
            return new ServicoDTO
            {
                Nome = request.Nome,
                Descricao = request.Descricao,
                ValorEstipulado = request.ValorEstipulado,
                Ativo = true
            };
        }
        private ServicoDTO TransformaParaDTO(int id, ServicoUpdateRequest request)
        {
            return new ServicoDTO
            {
                IdServico = id,
                Nome = request.Nome,
                Descricao = request.Descricao,
                ValorEstipulado = request.ValorEstipulado,
                Ativo = request.Ativo
            };
        }

        private ServicoResponse TransformaParaResponse(ServicoDTO response)
        {
            return new ServicoResponse
            {
                IdServico = response.IdServico,
                Nome = response.Nome,
                Descricao = response.Descricao,
                ValorEstipulado = response.ValorEstipulado,
                Ativo = response.Ativo
            };
        }
    }
}