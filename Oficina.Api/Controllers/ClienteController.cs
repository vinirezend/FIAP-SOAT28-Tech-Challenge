using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.DTOs;

namespace Oficina.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorIdAsync(int id)
        {
            var cliente = await _clienteService.ObterPorIdAsync(id);
            if (cliente is null)
                return NotFound();
            return Ok(cliente);
        }

        [HttpGet("documento/{documento}")]
        public async Task<IActionResult> ObterPorDocumentoAsync(string documento)
        {
            var cliente = await _clienteService.ObterPorDocumentoAsync(documento);
            if (cliente is null)
                return NotFound();
            return Ok(cliente);
        }
        
        [HttpGet]
        public async Task<IActionResult> ObterTodosAsync()
        {
            var clientes = await _clienteService.ObterTodosAsync();
            return Ok(clientes);
        }
        
        [HttpPost]
        public async Task<IActionResult> AdicionarAsync([FromBody] ClienteDTO cliente)
        {
            var clienteAdicionado = await _clienteService.AdicionarAsync(cliente);
            return Created($"/api/cliente/{clienteAdicionado.IdCliente}", clienteAdicionado);
        }       

        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarAsync(int id, [FromBody] ClienteDTO cliente)
        {
            cliente.IdCliente = id;
            var clienteAtualizado = await _clienteService.AtualizarAsync(cliente);
            if (clienteAtualizado is null)
                return NotFound();
            return Ok(clienteAtualizado);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletarAsync(int id)
        {
            var cliente = await _clienteService.ObterPorIdAsync(id);
            if (cliente is null)
                return NotFound();
            await _clienteService.RemoverAsync(id);
            return NoContent();
        }
    }
}