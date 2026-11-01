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
    private const string clienteNaoEncontrado = "Cliente não encontrado.";

    /// <summary>
    /// Obtém um cliente pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<ClienteResponse>, NotFound<string>>> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        await clienteService.ObterPorIdAsync(id, cancellationToken) is { } cliente
            ? TypedResults.Ok(TransformaParaResponse(cliente))
            : TypedResults.NotFound(clienteNaoEncontrado);

    /// <summary>
    /// Obtém um cliente pelo documento (CPF ou CNPJ).
    /// </summary>
    [HttpGet("documento/{documento}")]
    public async Task<Results<Ok<ClienteResponse>, NotFound<string>>> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken) =>
        await clienteService.ObterPorDocumentoAsync(documento, cancellationToken) is { } cliente
            ? TypedResults.Ok(TransformaParaResponse(cliente))
            : TypedResults.NotFound(clienteNaoEncontrado);

    /// <summary>
    /// Lista todos os clientes.
    /// </summary>
    [HttpGet]
    public async Task<Ok<IEnumerable<ClienteResponse>>> ObterTodosAsync(CancellationToken cancellationToken) =>
        TypedResults.Ok((await clienteService.ObterTodosAsync(cancellationToken)).Select(TransformaParaResponse));

    /// <summary>
    /// Cadastra um novo cliente; o tipo de documento é identificado a partir do CPF/CNPJ informado.
    /// </summary>
    [HttpPost]
    public async Task<Created<ClienteResponse>> AdicionarAsync(ClienteCreateRequest request, CancellationToken cancellationToken)
    {
        var cliente = await clienteService.AdicionarAsync(TransformaParaDTO(request), cancellationToken);
        return TypedResults.Created(Url.Action(nameof(ObterPorIdAsync), new { id = cliente.IdCliente }), TransformaParaResponse(cliente));
    }

    /// <summary>
    /// Atualiza nome, e-mail e telefone de um cliente. O documento não pode ser alterado.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<Results<Ok<ClienteResponse>, NotFound<string>>> AtualizarAsync(int id, ClienteUpdateRequest request, CancellationToken cancellationToken) =>
        await clienteService.AtualizarAsync(TransformaParaDTO(id, request), cancellationToken) is { } cliente
            ? TypedResults.Ok(TransformaParaResponse(cliente))
            : TypedResults.NotFound(clienteNaoEncontrado);

    /// <summary>
    /// Remove um cliente.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<Results<NoContent, NotFound<string>>> DeletarAsync(int id, CancellationToken cancellationToken)
    {
        if (await clienteService.ObterPorIdAsync(id, cancellationToken) is null)
            return TypedResults.NotFound(clienteNaoEncontrado);

        await clienteService.RemoverAsync(id, cancellationToken);
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
