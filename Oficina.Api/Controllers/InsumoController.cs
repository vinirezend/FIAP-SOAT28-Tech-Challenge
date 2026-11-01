using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.Insumo;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Controllers;

/// <summary>
/// Cadastro de insumos (peças e materiais) e controle de estoque e reserva.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InsumoController(IInsumoService insumoService) : ControllerBase
{
    private const string insumoNaoEncontrado = "Insumo não encontrado.";

    /// <summary>
    /// Obtém um insumo pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<InsumoResponse>, NotFound<string>>> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        await insumoService.ObterPorIdAsync(id, cancellationToken) is { } insumo
            ? TypedResults.Ok(TransformaParaResponse(insumo))
            : TypedResults.NotFound(insumoNaoEncontrado);

    /// <summary>
    /// Lista todos os insumos.
    /// </summary>
    [HttpGet]
    public async Task<Ok<IEnumerable<InsumoResponse>>> ObterTodosAsync(CancellationToken cancellationToken) =>
        TypedResults.Ok((await insumoService.ObterTodosAsync(cancellationToken)).Select(TransformaParaResponse));

    /// <summary>
    /// Pesquisa insumos pelo termo informado.
    /// </summary>
    [HttpGet("pesquisar/{termo}")]
    public async Task<Ok<IEnumerable<InsumoResponse>>> PesquisarAsync(string termo, CancellationToken cancellationToken) =>
        TypedResults.Ok((await insumoService.PesquisarAsync(termo, cancellationToken)).Select(TransformaParaResponse));

    /// <summary>
    /// Cadastra um novo insumo com a quantidade inicial em estoque.
    /// </summary>
    [HttpPost]
    public async Task<Created<InsumoResponse>> AdicionarAsync(InsumoCreateRequest request, CancellationToken cancellationToken)
    {
        var insumo = await insumoService.AdicionarAsync(TransformaParaDTO(request), cancellationToken);
        return TypedResults.Created(Url.Action(nameof(ObterPorIdAsync), new { id = insumo.IdInsumo }), TransformaParaResponse(insumo));
    }

    /// <summary>
    /// Atualiza os dados de um insumo, incluindo quantidades em estoque e reservada.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<Results<Ok<InsumoResponse>, NotFound<string>>> AtualizarAsync(int id, InsumoUpdateRequest request, CancellationToken cancellationToken) =>
        await insumoService.AtualizarAsync(TransformaParaDTO(id, request), cancellationToken) is { } insumo
            ? TypedResults.Ok(TransformaParaResponse(insumo))
            : TypedResults.NotFound(insumoNaoEncontrado);

    /// <summary>
    /// Remove um insumo.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<Results<NoContent, NotFound<string>>> DeletarAsync(int id, CancellationToken cancellationToken)
    {
        if (await insumoService.ObterPorIdAsync(id, cancellationToken) is null)
            return TypedResults.NotFound(insumoNaoEncontrado);

        await insumoService.RemoverAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Dá entrada da quantidade informada no estoque do insumo.
    /// </summary>
    [HttpPatch("{idInsumo:int}/adicionar-estoque/{quantidade:int}")]
    public async Task<Ok<InsumoQuantidadeResponse>> AdicionarEstoqueAsync(int idInsumo, int quantidade, CancellationToken cancellationToken) =>
        TypedResults.Ok(TransformaParaResponse(await insumoService.AdicionarEstoqueAsync(idInsumo, quantidade, cancellationToken)));

    /// <summary>
    /// Dá baixa da quantidade informada no estoque do insumo.
    /// </summary>
    [HttpPatch("{idInsumo:int}/retirar-estoque/{quantidade:int}")]
    public async Task<Ok<InsumoQuantidadeResponse>> RetirarEstoqueAsync(int idInsumo, int quantidade, CancellationToken cancellationToken) =>
        TypedResults.Ok(TransformaParaResponse(await insumoService.RetirarEstoqueAsync(idInsumo, quantidade, cancellationToken)));

    /// <summary>
    /// Reserva a quantidade informada do insumo.
    /// </summary>
    [HttpPatch("{idInsumo:int}/adicionar-reserva/{quantidade:int}")]
    public async Task<Ok<InsumoQuantidadeResponse>> AdicionarReservaAsync(int idInsumo, int quantidade, CancellationToken cancellationToken) =>
        TypedResults.Ok(TransformaParaResponse(await insumoService.AdicionarReservaAsync(idInsumo, quantidade, cancellationToken)));

    /// <summary>
    /// Libera a quantidade informada da reserva do insumo.
    /// </summary>
    [HttpPatch("{idInsumo:int}/retirar-reserva/{quantidade:int}")]
    public async Task<Ok<InsumoQuantidadeResponse>> RetirarReservaAsync(int idInsumo, int quantidade, CancellationToken cancellationToken) =>
        TypedResults.Ok(TransformaParaResponse(await insumoService.RetirarReservaAsync(idInsumo, quantidade, cancellationToken)));

    private static InsumoDTO TransformaParaDTO(InsumoCreateRequest request) => new()
    {
        Nome = request.Nome,
        Descricao = request.Descricao ?? string.Empty,
        ValorUnitario = request.ValorUnitario,
        QuantidadeEstoque = request.QuantidadeEstoque
    };

    private static InsumoDTO TransformaParaDTO(int id, InsumoUpdateRequest request) => new()
    {
        IdInsumo = id,
        Nome = request.Nome,
        Descricao = request.Descricao ?? string.Empty,
        ValorUnitario = request.ValorUnitario,
        Ativo = request.Ativo,
        QuantidadeEstoque = request.QuantidadeEstoque,
        QuantidadeReservada = request.QuantidadeReservada
    };

    private static InsumoResponse TransformaParaResponse(InsumoDTO dto) => new()
    {
        IdInsumo = dto.IdInsumo,
        Nome = dto.Nome,
        Descricao = dto.Descricao,
        ValorUnitario = dto.ValorUnitario,
        Ativo = dto.Ativo,
        QuantidadeEstoque = dto.QuantidadeEstoque,
        QuantidadeReservada = dto.QuantidadeReservada
    };

    private static InsumoQuantidadeResponse TransformaParaResponse(InsumoQuantidadeDTO dto) => new()
    {
        Nome = dto.Nome,
        QuantidadeEstoque = dto.QuantidadeEstoque,
        QuantidadeReservada = dto.QuantidadeReservada
    };
}
