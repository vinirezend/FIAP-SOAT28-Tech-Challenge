using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.OrdemServico;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdemServicoController(IOrdemServicoService ordemServicoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObterTodosAsync()
    {
        var ordensServico = await ordemServicoService.ObterTodosAsync();
        return Ok(ordensServico.Select(TransformaParaResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorIdAsync(int id)
    {
        var ordemServico = await ordemServicoService.ObterPorIdAsync(id);
        if (ordemServico is null)
            return NotFound();
        return Ok(TransformaParaResponse(ordemServico));
    }

    [HttpGet("veiculo/{idVeiculo:int}")]
    public async Task<IActionResult> ObterPorVeiculoAsync(int idVeiculo)
    {
        var ordensServico = await ordemServicoService.ObterPorVeiculoAsync(idVeiculo);
        return Ok(ordensServico.Select(TransformaParaResponse));
    }

    /// <summary>
    /// Abre uma OS com status Recebida, registrada em nome do colaborador autenticado.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AdicionarAsync([FromBody] OrdemServicoRequest request)
    {
        try
        {
            var ordemServicoAdicionada = await ordemServicoService.AdicionarAsync(new OrdemServicoDTO { IdVeiculo = request.IdVeiculo });
            return Created($"/api/ordemservico/{ordemServicoAdicionada.IdOs}", TransformaParaResponse(ordemServicoAdicionada));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> AtualizarAsync(int id, [FromBody] OrdemServicoRequest request)
    {
        try
        {
            var ordemServicoAtualizada = await ordemServicoService.AtualizarAsync(new OrdemServicoDTO { IdOs = id, IdVeiculo = request.IdVeiculo });
            if (ordemServicoAtualizada is null)
                return NotFound();
            return Ok(TransformaParaResponse(ordemServicoAtualizada));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:int}/status")]
    public async Task<IActionResult> ObterHistoricoStatusAsync(int id)
    {
        var historico = await ordemServicoService.ObterHistoricoStatusAsync(id);
        if (historico is null)
            return NotFound();
        return Ok(historico.Select(TransformaParaResponse));
    }

    /// <summary>
    /// Avança a OS para o próximo status do fluxo, registrado em nome do colaborador autenticado.
    /// </summary>
    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> AlterarStatusAsync(int id, [FromBody] AlterarStatusRequest request)
    {
        try
        {
            var ordemServico = await ordemServicoService.AlterarStatusAsync(id, request.IdStatus);
            if (ordemServico is null)
                return NotFound();
            return Ok(TransformaParaResponse(ordemServico));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletarAsync(int id)
    {
        var ordemServico = await ordemServicoService.ObterPorIdAsync(id);
        if (ordemServico is null)
            return NotFound();

        try
        {
            await ordemServicoService.RemoverAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    private static OrdemServicoResponse TransformaParaResponse(OrdemServicoDTO dto)
    {
        return new OrdemServicoResponse
        {
            IdOs = dto.IdOs,
            IdVeiculo = dto.IdVeiculo,
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            Valor = dto.Valor,
            IdStatus = dto.IdStatus,
            Status = dto.Status
        };
    }

    private static OrdemServicoStatusResponse TransformaParaResponse(OrdemServicoStatusDTO dto)
    {
        return new OrdemServicoStatusResponse
        {
            IdOsStatus = dto.IdOsStatus,
            IdStatus = dto.IdStatus,
            Status = dto.Status,
            IdColaborador = dto.IdColaborador,
            DataInclusao = dto.DataInclusao
        };
    }
}
