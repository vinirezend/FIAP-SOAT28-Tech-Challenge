using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.Servico;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Controllers;

/// <summary>
/// Catálogo de serviços oferecidos pela oficina.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ServicoController(IServicoService servicoService) : ControllerBase
{
    /// <summary>
    /// Obtém um serviço pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<ServicoResponse>, NotFound>> ObterPorIdAsync(int id) =>
        await servicoService.ObterPorIdAsync(id) is { } servico
            ? TypedResults.Ok(TransformaParaResponse(servico))
            : TypedResults.NotFound();

    /// <summary>
    /// Lista todos os serviços.
    /// </summary>
    [HttpGet]
    public async Task<Ok<IEnumerable<ServicoResponse>>> ObterTodosAsync() =>
        TypedResults.Ok((await servicoService.ObterTodosAsync()).Select(TransformaParaResponse));

    /// <summary>
    /// Pesquisa serviços pelo termo informado.
    /// </summary>
    [HttpGet("pesquisar/{termo}")]
    public async Task<Ok<IEnumerable<ServicoResponse>>> PesquisarAsync(string termo) =>
        TypedResults.Ok((await servicoService.PesquisarAsync(termo)).Select(TransformaParaResponse));

    /// <summary>
    /// Cadastra um novo serviço, já ativo.
    /// </summary>
    [HttpPost]
    public async Task<Created<ServicoResponse>> AdicionarAsync(ServicoCreateRequest request)
    {
        var servico = await servicoService.AdicionarAsync(TransformaParaDTO(request));
        return TypedResults.Created(Url.Action(nameof(ObterPorIdAsync), new { id = servico.IdServico }), TransformaParaResponse(servico));
    }

    /// <summary>
    /// Atualiza os dados de um serviço, incluindo se está ativo.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<Results<Ok<ServicoResponse>, NotFound>> AtualizarAsync(int id, ServicoUpdateRequest request) =>
        await servicoService.AtualizarAsync(TransformaParaDTO(id, request)) is { } servico
            ? TypedResults.Ok(TransformaParaResponse(servico))
            : TypedResults.NotFound();

    /// <summary>
    /// Remove um serviço.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<Results<NoContent, NotFound>> DeletarAsync(int id)
    {
        if (await servicoService.ObterPorIdAsync(id) is null)
            return TypedResults.NotFound();

        await servicoService.RemoverAsync(id);
        return TypedResults.NoContent();
    }

    private static ServicoDTO TransformaParaDTO(ServicoCreateRequest request) => new()
    {
        Nome = request.Nome,
        Descricao = request.Descricao ?? string.Empty,
        ValorEstipulado = request.ValorEstipulado,
        Ativo = true
    };

    private static ServicoDTO TransformaParaDTO(int id, ServicoUpdateRequest request) => new()
    {
        IdServico = id,
        Nome = request.Nome,
        Descricao = request.Descricao ?? string.Empty,
        ValorEstipulado = request.ValorEstipulado,
        Ativo = request.Ativo
    };

    private static ServicoResponse TransformaParaResponse(ServicoDTO dto) => new()
    {
        IdServico = dto.IdServico,
        Nome = dto.Nome,
        Descricao = dto.Descricao,
        ValorEstipulado = dto.ValorEstipulado,
        Ativo = dto.Ativo
    };
}
