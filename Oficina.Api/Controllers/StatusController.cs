using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.Status;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Controllers;

/// <summary>
/// Status possíveis de uma OS. Lista fixa (carga inicial): somente consulta.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StatusController(IStatusService statusService) : ControllerBase
{
    /// <summary>
    /// Lista todos os status de OS.
    /// </summary>
    [HttpGet]
    public async Task<Ok<IEnumerable<StatusResponse>>> ObterTodosAsync() =>
        TypedResults.Ok((await statusService.ObterTodosAsync()).Select(TransformaParaResponse));

    /// <summary>
    /// Obtém um status pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<StatusResponse>, NotFound>> ObterPorIdAsync(int id) =>
        await statusService.ObterPorIdAsync(id) is { } status
            ? TypedResults.Ok(TransformaParaResponse(status))
            : TypedResults.NotFound();

    private static StatusResponse TransformaParaResponse(StatusDTO dto) => new()
    {
        IdStatus = dto.IdStatus,
        Nome = dto.Nome
    };
}
