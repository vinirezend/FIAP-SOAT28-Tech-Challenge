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
    private const string servicoNaoEncontrado = "Serviço não encontrado.";

    /// <summary>
    /// Obtém um serviço pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<ServicoResponse>, NotFound<string>>> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        await servicoService.ObterPorIdAsync(id, cancellationToken) is { } servico
            ? TypedResults.Ok(TransformaParaResponse(servico))
            : TypedResults.NotFound(servicoNaoEncontrado);

    /// <summary>
    /// Lista todos os serviços.
    /// </summary>
    [HttpGet]
    public async Task<Ok<IEnumerable<ServicoResponse>>> ObterTodosAsync(CancellationToken cancellationToken) =>
        TypedResults.Ok((await servicoService.ObterTodosAsync(cancellationToken)).Select(TransformaParaResponse));

    /// <summary>
    /// Pesquisa serviços pelo termo informado.
    /// </summary>
    [HttpGet("pesquisar/{termo}")]
    public async Task<Ok<IEnumerable<ServicoResponse>>> PesquisarAsync(string termo, CancellationToken cancellationToken) =>
        TypedResults.Ok((await servicoService.PesquisarAsync(termo, cancellationToken)).Select(TransformaParaResponse));

    /// <summary>
    /// Cadastra um novo serviço, já ativo.
    /// </summary>
    [HttpPost]
    public async Task<Created<ServicoResponse>> AdicionarAsync(ServicoCreateRequest request, CancellationToken cancellationToken)
    {
        var servico = await servicoService.AdicionarAsync(TransformaParaDTO(request), cancellationToken);
        return TypedResults.Created(Url.Action(nameof(ObterPorIdAsync), new { id = servico.IdServico }), TransformaParaResponse(servico));
    }

    /// <summary>
    /// Atualiza os dados de um serviço, incluindo se está ativo.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<Results<Ok<ServicoResponse>, NotFound<string>>> AtualizarAsync(int id, ServicoUpdateRequest request, CancellationToken cancellationToken) =>
        await servicoService.AtualizarAsync(TransformaParaDTO(id, request), cancellationToken) is { } servico
            ? TypedResults.Ok(TransformaParaResponse(servico))
            : TypedResults.NotFound(servicoNaoEncontrado);

    /// <summary>
    /// Remove um serviço.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<Results<NoContent, NotFound<string>>> DeletarAsync(int id, CancellationToken cancellationToken)
    {
        if (await servicoService.ObterPorIdAsync(id, cancellationToken) is null)
            return TypedResults.NotFound(servicoNaoEncontrado);

        await servicoService.RemoverAsync(id, cancellationToken);
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
