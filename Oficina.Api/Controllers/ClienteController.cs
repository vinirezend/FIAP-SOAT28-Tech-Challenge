using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.DTOs;
using Oficina.Api.DTO.Cliente;
using Microsoft.AspNetCore.Authorization;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ClienteController(IClienteService clienteService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorIdAsync(int id)
    {
        var cliente = await clienteService.ObterPorIdAsync(id);
        if (cliente is null)
            return NotFound();
        return Ok(TransformaParaResponse(cliente));
    }

    [HttpGet("documento/{documento}")]
    public async Task<IActionResult> ObterPorDocumentoAsync(string documento)
    {
        var cliente = await clienteService.ObterPorDocumentoAsync(documento);
        if (cliente is null)
            return NotFound();
        return Ok(TransformaParaResponse(cliente));
    }
    
    [HttpGet]
    public async Task<IActionResult> ObterTodosAsync()
    {
        var clientes = await clienteService.ObterTodosAsync();
        return Ok(clientes.Select(c => TransformaParaResponse(c)));
    }
    
    [HttpPost]
    public async Task<IActionResult> AdicionarAsync([FromBody] ClienteCreateRequest request)
    {
        var cliente = TransformaParaDTO(request);
        var clienteAdicionado = await clienteService.AdicionarAsync(cliente);
        return Created($"/api/cliente/{clienteAdicionado.IdCliente}", TransformaParaResponse(clienteAdicionado));
    }       

    [HttpPut("{id:int}")]
    public async Task<IActionResult> AtualizarAsync(int id, [FromBody] ClienteUpdateRequest request)
    {
        var cliente = TransformaParaDTO(id, request);
        var clienteAtualizado = await clienteService.AtualizarAsync(cliente);
        if (clienteAtualizado is null)
            return NotFound();
        return Ok(TransformaParaResponse(clienteAtualizado));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletarAsync(int id)
    {
        var cliente = await clienteService.ObterPorIdAsync(id);
        if (cliente is null)
            return NotFound();
        await clienteService.RemoverAsync(id);
        return NoContent();
    }

    private static ClienteDTO TransformaParaDTO(ClienteCreateRequest request) { 
        return new ClienteDTO
        {
            Nome = request.Nome,
            Documento = request.Documento,
            Email = request.Email,
            Telefone = request.Telefone
        };
     }
    private static ClienteDTO TransformaParaDTO(int id, ClienteUpdateRequest request) { 
        return new ClienteDTO
        {
            IdCliente = id,
            Nome = request.Nome,
            Email = request.Email,
            Telefone = request.Telefone
        };
    }
    private static ClienteResponse TransformaParaResponse(ClienteDTO dto) { 
        return new ClienteResponse
        {
            IdCliente = dto.IdCliente,
            Nome = dto.Nome,
            Documento = dto.Documento,
            TipoDocumento = dto.TipoDocumento,
            Email = dto.Email,
            Telefone = dto.Telefone
        };
    }
}