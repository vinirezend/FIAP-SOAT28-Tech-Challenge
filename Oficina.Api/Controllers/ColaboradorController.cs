using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> ObterTodosAsync()
    {
        var colaboradores = await colaboradorService.ObterTodosAsync();
        return Ok(colaboradores.Select(TransformaParaResponse));
    }

    /// <summary>
    /// Obtém um colaborador pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorIdAsync(int id)
    {
        var colaborador = await colaboradorService.ObterPorIdAsync(id);
        if (colaborador is null)
            return NotFound();
        return Ok(TransformaParaResponse(colaborador));
    }

    /// <summary>
    /// Cadastra um novo colaborador.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AdicionarAsync([FromBody] ColaboradorCreateRequest request)
    {
        try
        {
            var colaboradorAdicionado = await colaboradorService.AdicionarAsync(TransformaParaDTO(request));
            return Created($"/api/colaborador/{colaboradorAdicionado.IdColaborador}", TransformaParaResponse(colaboradorAdicionado));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Atualiza os dados de um colaborador. O CPF não pode ser alterado.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> AtualizarAsync(int id, [FromBody] ColaboradorUpdateRequest request)
    {
        try
        {
            var colaboradorAtualizado = await colaboradorService.AtualizarAsync(TransformaParaDTO(id, request));
            if (colaboradorAtualizado is null)
                return NotFound();
            return Ok(TransformaParaResponse(colaboradorAtualizado));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove um colaborador. Retorna 409 quando há registros vinculados que impedem a remoção.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletarAsync(int id)
    {
        var colaborador = await colaboradorService.ObterPorIdAsync(id);
        if (colaborador is null)
            return NotFound();

        try
        {
            await colaboradorService.RemoverAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    private static ColaboradorDTO TransformaParaDTO(ColaboradorCreateRequest request)
    {
        return new ColaboradorDTO
        {
            Nome = request.Nome,
            Cpf = request.Cpf,
            Email = request.Email,
            IdDepartamento = request.IdDepartamento,
            IdUsuario = request.IdUsuario
        };
    }

    private static ColaboradorDTO TransformaParaDTO(int id, ColaboradorUpdateRequest request)
    {
        return new ColaboradorDTO
        {
            IdColaborador = id,
            Nome = request.Nome,
            Email = request.Email,
            IdDepartamento = request.IdDepartamento,
            IdUsuario = request.IdUsuario,
            Ativo = request.Ativo
        };
    }

    private static ColaboradorResponse TransformaParaResponse(ColaboradorDTO dto)
    {
        return new ColaboradorResponse
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
}
