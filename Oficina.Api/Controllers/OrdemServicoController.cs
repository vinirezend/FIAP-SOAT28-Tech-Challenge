using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.OrdemServico;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Controllers;

/// <summary>
/// Ordens de serviço: abertura, fluxo de status e itens (serviços e insumos) da OS.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdemServicoController(IOrdemServicoService ordemServicoService) : ControllerBase
{
    /// <summary>
    /// Lista todas as ordens de serviço.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterTodosAsync()
    {
        var ordensServico = await ordemServicoService.ObterTodosAsync();
        return Ok(ordensServico.Select(TransformaParaResponse));
    }

    /// <summary>
    /// Obtém uma ordem de serviço pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorIdAsync(int id)
    {
        var ordemServico = await ordemServicoService.ObterPorIdAsync(id);
        if (ordemServico is null)
            return NotFound();
        return Ok(TransformaParaResponse(ordemServico));
    }

    /// <summary>
    /// Lista as ordens de serviço de um veículo.
    /// </summary>
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

    /// <summary>
    /// Altera o veículo vinculado à OS.
    /// </summary>
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

    /// <summary>
    /// Histórico de status da OS, com data e colaborador de cada mudança.
    /// </summary>
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

    /// <summary>
    /// Lista os serviços incluídos na OS.
    /// </summary>
    [HttpGet("{id:int}/servicos")]
    public async Task<IActionResult> ObterServicosAsync(int id)
    {
        var servicos = await ordemServicoService.ObterServicosAsync(id);
        if (servicos is null)
            return NotFound();
        return Ok(servicos.Select(TransformaParaResponse));
    }

    /// <summary>
    /// Inclui um serviço na OS com a quantidade informada.
    /// </summary>
    [HttpPost("{id:int}/servicos")]
    public async Task<IActionResult> AdicionarServicoAsync(int id, [FromBody] AdicionarServicoRequest request)
    {
        try
        {
            var servico = await ordemServicoService.AdicionarServicoAsync(id, request.IdServico, request.Quantidade);
            if (servico is null)
                return NotFound();
            return Created($"/api/ordemservico/{id}/servicos/{servico.IdOsServico}", TransformaParaResponse(servico));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Altera a quantidade de um serviço da OS.
    /// </summary>
    [HttpPut("{id:int}/servicos/{idOsServico:int}")]
    public async Task<IActionResult> AlterarQuantidadeServicoAsync(int id, int idOsServico, [FromBody] AlterarQuantidadeServicoRequest request)
    {
        try
        {
            var servico = await ordemServicoService.AlterarQuantidadeServicoAsync(id, idOsServico, request.Quantidade);
            if (servico is null)
                return NotFound();
            return Ok(TransformaParaResponse(servico));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove um serviço da OS.
    /// </summary>
    [HttpDelete("{id:int}/servicos/{idOsServico:int}")]
    public async Task<IActionResult> RemoverServicoAsync(int id, int idOsServico)
    {
        try
        {
            await ordemServicoService.RemoverServicoAsync(id, idOsServico);
            return NoContent();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Lista os insumos incluídos na OS.
    /// </summary>
    [HttpGet("{id:int}/insumos")]
    public async Task<IActionResult> ObterInsumosAsync(int id)
    {
        var insumos = await ordemServicoService.ObterInsumosAsync(id);
        if (insumos is null)
            return NotFound();
        return Ok(insumos.Select(TransformaParaResponse));
    }

    /// <summary>
    /// Inclui um insumo na OS com a quantidade informada.
    /// </summary>
    [HttpPost("{id:int}/insumos")]
    public async Task<IActionResult> AdicionarInsumoAsync(int id, [FromBody] AdicionarInsumoRequest request)
    {
        try
        {
            var insumo = await ordemServicoService.AdicionarInsumoAsync(id, request.IdInsumo, request.Quantidade);
            if (insumo is null)
                return NotFound();
            return Created($"/api/ordemservico/{id}/insumos/{insumo.IdOsInsumo}", TransformaParaResponse(insumo));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Altera a quantidade de um insumo da OS.
    /// </summary>
    [HttpPut("{id:int}/insumos/{idOsInsumo:int}")]
    public async Task<IActionResult> AlterarQuantidadeInsumoAsync(int id, int idOsInsumo, [FromBody] AlterarQuantidadeInsumoRequest request)
    {
        try
        {
            var insumo = await ordemServicoService.AlterarQuantidadeInsumoAsync(id, idOsInsumo, request.Quantidade);
            if (insumo is null)
                return NotFound();
            return Ok(TransformaParaResponse(insumo));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove um insumo da OS.
    /// </summary>
    [HttpDelete("{id:int}/insumos/{idOsInsumo:int}")]
    public async Task<IActionResult> RemoverInsumoAsync(int id, int idOsInsumo)
    {
        try
        {
            await ordemServicoService.RemoverInsumoAsync(id, idOsInsumo);
            return NoContent();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove uma ordem de serviço.
    /// </summary>
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

    private static OrdemServicoServicoResponse TransformaParaResponse(OrdemServicoServicoDTO dto)
    {
        return new OrdemServicoServicoResponse
        {
            IdOsServico = dto.IdOsServico,
            IdOs = dto.IdOs,
            IdServico = dto.IdServico,
            Quantidade = dto.Quantidade,
            ValorCobrado = dto.ValorCobrado,
            ValorTotal = dto.ValorTotal
        };
    }

    private static OrdemServicoInsumoResponse TransformaParaResponse(OrdemServicoInsumoDTO dto)
    {
        return new OrdemServicoInsumoResponse
        {
            IdOsInsumo = dto.IdOsInsumo,
            IdOs = dto.IdOs,
            IdInsumo = dto.IdInsumo,
            Quantidade = dto.Quantidade,
            ValorCobrado = dto.ValorCobrado,
            ValorTotal = dto.ValorTotal
        };
    }
}
