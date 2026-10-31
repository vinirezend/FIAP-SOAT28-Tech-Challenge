using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.Cliente;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Controllers;

/// <summary>
/// Cadastro de clientes da oficina (pessoa física ou jurídica).
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ClienteController(IClienteService clienteService) : ControllerBase
{
    /// <summary>
    /// Obtém um cliente pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<ClienteResponse>, NotFound>> ObterPorIdAsync(int id) =>
        await clienteService.ObterPorIdAsync(id) is { } cliente
            ? TypedResults.Ok(TransformaParaResponse(cliente))
            : TypedResults.NotFound();

    /// <summary>
    /// Obtém um cliente pelo documento (CPF ou CNPJ).
    /// </summary>
    [HttpGet("documento/{documento}")]
    public async Task<Results<Ok<ClienteResponse>, NotFound>> ObterPorDocumentoAsync(string documento) =>
        await clienteService.ObterPorDocumentoAsync(documento) is { } cliente
            ? TypedResults.Ok(TransformaParaResponse(cliente))
            : TypedResults.NotFound();

    /// <summary>
    /// Lista todos os clientes.
    /// </summary>
    [HttpGet]
    public async Task<Ok<IEnumerable<ClienteResponse>>> ObterTodosAsync() =>
        TypedResults.Ok((await clienteService.ObterTodosAsync()).Select(TransformaParaResponse));

    /// <summary>
    /// Cadastra um novo cliente; o tipo de documento é identificado a partir do CPF/CNPJ informado.
    /// </summary>
    [HttpPost]
    public async Task<Created<ClienteResponse>> AdicionarAsync(ClienteCreateRequest request)
    {
        var cliente = await clienteService.AdicionarAsync(TransformaParaDTO(request));
        return TypedResults.Created(Url.Action(nameof(ObterPorIdAsync), new { id = cliente.IdCliente }), TransformaParaResponse(cliente));
    }

    /// <summary>
    /// Atualiza nome, e-mail e telefone de um cliente. O documento não pode ser alterado.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<Results<Ok<ClienteResponse>, NotFound>> AtualizarAsync(int id, ClienteUpdateRequest request) =>
        await clienteService.AtualizarAsync(TransformaParaDTO(id, request)) is { } cliente
            ? TypedResults.Ok(TransformaParaResponse(cliente))
            : TypedResults.NotFound();

    /// <summary>
    /// Remove um cliente.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<Results<NoContent, NotFound>> DeletarAsync(int id)
    {
        if (await clienteService.ObterPorIdAsync(id) is null)
            return TypedResults.NotFound();

        await clienteService.RemoverAsync(id);
        return TypedResults.NoContent();
    }

    private static ClienteDTO TransformaParaDTO(ClienteCreateRequest request) => new()
    {
        Nome = request.Nome,
        Documento = request.Documento,
        Email = request.Email,
        Telefone = request.Telefone
    };

    private static ClienteDTO TransformaParaDTO(int id, ClienteUpdateRequest request) => new()
    {
        IdCliente = id,
        Nome = request.Nome,
        Email = request.Email,
        Telefone = request.Telefone
    };

    private static ClienteResponse TransformaParaResponse(ClienteDTO dto) => new()
    {
        IdCliente = dto.IdCliente,
        Nome = dto.Nome,
        Documento = dto.Documento,
        TipoDocumento = dto.TipoDocumento,
        Email = dto.Email,
        Telefone = dto.Telefone
    };
}
