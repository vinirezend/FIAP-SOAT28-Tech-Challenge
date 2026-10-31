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
    /// <summary>
    /// Obtém um insumo pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<InsumoResponse>, NotFound>> ObterPorIdAsync(int id) =>
        await insumoService.ObterPorIdAsync(id) is { } insumo
            ? TypedResults.Ok(TransformaParaResponse(insumo))
            : TypedResults.NotFound();

    /// <summary>
    /// Lista todos os insumos.
    /// </summary>
    [HttpGet]
    public async Task<Ok<IEnumerable<InsumoResponse>>> ObterTodosAsync() =>
        TypedResults.Ok((await insumoService.ObterTodosAsync()).Select(TransformaParaResponse));

    /// <summary>
    /// Pesquisa insumos pelo termo informado.
    /// </summary>
    [HttpGet("pesquisar/{termo}")]
    public async Task<Ok<IEnumerable<InsumoResponse>>> PesquisarAsync(string termo) =>
        TypedResults.Ok((await insumoService.PesquisarAsync(termo)).Select(TransformaParaResponse));

    /// <summary>
    /// Cadastra um novo insumo com a quantidade inicial em estoque.
    /// </summary>
    [HttpPost]
    public async Task<Created<InsumoResponse>> AdicionarAsync(InsumoCreateRequest request)
    {
        var insumo = await insumoService.AdicionarAsync(TransformaParaDTO(request));
        return TypedResults.Created(Url.Action(nameof(ObterPorIdAsync), new { id = insumo.IdInsumo }), TransformaParaResponse(insumo));
    }

    /// <summary>
    /// Atualiza os dados de um insumo, incluindo quantidades em estoque e reservada.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<Results<Ok<InsumoResponse>, NotFound>> AtualizarAsync(int id, InsumoUpdateRequest request) =>
        await insumoService.AtualizarAsync(TransformaParaDTO(id, request)) is { } insumo
            ? TypedResults.Ok(TransformaParaResponse(insumo))
            : TypedResults.NotFound();

    /// <summary>
    /// Remove um insumo.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<Results<NoContent, NotFound>> DeletarAsync(int id)
    {
        if (await insumoService.ObterPorIdAsync(id) is null)
            return TypedResults.NotFound();

        await insumoService.RemoverAsync(id);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Dá entrada da quantidade informada no estoque do insumo.
    /// </summary>
    [HttpPatch("{idInsumo:int}/adicionar-estoque/{quantidade:int}")]
    public async Task<Ok<InsumoQuantidadeResponse>> AdicionarEstoqueAsync(int idInsumo, int quantidade) =>
        TypedResults.Ok(TransformaParaResponse(await insumoService.AdicionarEstoqueAsync(idInsumo, quantidade)));

    /// <summary>
    /// Dá baixa da quantidade informada no estoque do insumo.
    /// </summary>
    [HttpPatch("{idInsumo:int}/retirar-estoque/{quantidade:int}")]
    public async Task<Ok<InsumoQuantidadeResponse>> RetirarEstoqueAsync(int idInsumo, int quantidade) =>
        TypedResults.Ok(TransformaParaResponse(await insumoService.RetirarEstoqueAsync(idInsumo, quantidade)));

    /// <summary>
    /// Reserva a quantidade informada do insumo.
    /// </summary>
    [HttpPatch("{idInsumo:int}/adicionar-reserva/{quantidade:int}")]
    public async Task<Ok<InsumoQuantidadeResponse>> AdicionarReservaAsync(int idInsumo, int quantidade) =>
        TypedResults.Ok(TransformaParaResponse(await insumoService.AdicionarReservaAsync(idInsumo, quantidade)));

    /// <summary>
    /// Libera a quantidade informada da reserva do insumo.
    /// </summary>
    [HttpPatch("{idInsumo:int}/retirar-reserva/{quantidade:int}")]
    public async Task<Ok<InsumoQuantidadeResponse>> RetirarReservaAsync(int idInsumo, int quantidade) =>
        TypedResults.Ok(TransformaParaResponse(await insumoService.RetirarReservaAsync(idInsumo, quantidade)));

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
