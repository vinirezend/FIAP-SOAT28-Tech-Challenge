using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.Colaborador;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Controllers;

/// <summary>
/// Cadastro de colaboradores da oficina e vínculo com departamento e usuário de acesso.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ColaboradorController(IColaboradorService colaboradorService) : ControllerBase
{
    /// <summary>
    /// Lista todos os colaboradores.
    /// </summary>
    [HttpGet]
    public async Task<Ok<IEnumerable<ColaboradorResponse>>> ObterTodosAsync(CancellationToken cancellationToken) =>
        TypedResults.Ok((await colaboradorService.ObterTodosAsync(cancellationToken)).Select(TransformaParaResponse));

    /// <summary>
    /// Obtém um colaborador pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<ColaboradorResponse>, NotFound>> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        await colaboradorService.ObterPorIdAsync(id, cancellationToken) is { } colaborador
            ? TypedResults.Ok(TransformaParaResponse(colaborador))
            : TypedResults.NotFound();

    /// <summary>
    /// Cadastra um novo colaborador.
    /// </summary>
    [HttpPost]
    public async Task<Created<ColaboradorResponse>> AdicionarAsync(ColaboradorCreateRequest request, CancellationToken cancellationToken)
    {
        var colaborador = await colaboradorService.AdicionarAsync(TransformaParaDTO(request), cancellationToken);
        return TypedResults.Created(Url.Action(nameof(ObterPorIdAsync), new { id = colaborador.IdColaborador }), TransformaParaResponse(colaborador));
    }

    /// <summary>
    /// Atualiza os dados de um colaborador. O CPF não pode ser alterado.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<Results<Ok<ColaboradorResponse>, NotFound>> AtualizarAsync(int id, ColaboradorUpdateRequest request, CancellationToken cancellationToken) =>
        await colaboradorService.AtualizarAsync(TransformaParaDTO(id, request), cancellationToken) is { } colaborador
            ? TypedResults.Ok(TransformaParaResponse(colaborador))
            : TypedResults.NotFound();

    /// <summary>
    /// Remove um colaborador. Retorna 409 quando há registros vinculados que impedem a remoção.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeletarAsync(int id, CancellationToken cancellationToken)
    {
        if (await colaboradorService.ObterPorIdAsync(id, cancellationToken) is null)
            return TypedResults.NotFound();

        try
        {
            await colaboradorService.RemoverAsync(id, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }

    private static ColaboradorDTO TransformaParaDTO(ColaboradorCreateRequest request) => new()
    {
        Nome = request.Nome,
        Cpf = request.Cpf,
        Email = request.Email,
        IdDepartamento = request.IdDepartamento,
        IdUsuario = request.IdUsuario
    };

    private static ColaboradorDTO TransformaParaDTO(int id, ColaboradorUpdateRequest request) => new()
    {
        IdColaborador = id,
        Nome = request.Nome,
        Email = request.Email,
        IdDepartamento = request.IdDepartamento,
        IdUsuario = request.IdUsuario,
        Ativo = request.Ativo
    };

    private static ColaboradorResponse TransformaParaResponse(ColaboradorDTO dto) => new()
    {
        IdColaborador = dto.IdColaborador,
        Nome = dto.Nome,
        Cpf = dto.Cpf,
        Email = dto.Email,
        Ativo = dto.Ativo,
        IdDepartamento = dto.IdDepartamento,
        IdUsuario = dto.IdUsuario
    };
}
