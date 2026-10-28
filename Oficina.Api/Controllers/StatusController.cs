using Microsoft.AspNetCore.Authorization;
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
    [HttpGet]
    public async Task<IActionResult> ObterTodosAsync()
    {
        var status = await statusService.ObterTodosAsync();
        return Ok(status.Select(TransformaParaResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorIdAsync(int id)
    {
        var status = await statusService.ObterPorIdAsync(id);
        if (status is null)
            return NotFound();
        return Ok(TransformaParaResponse(status));
    }

    private static StatusResponse TransformaParaResponse(StatusDTO dto)
    {
        return new StatusResponse
        {
            IdStatus = dto.IdStatus,
            Nome = dto.Nome
        };
    }
}
