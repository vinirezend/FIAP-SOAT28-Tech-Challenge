using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
    private const string osNaoEncontrada = "Ordem de serviço não encontrada.";
    /// <summary>
    /// Lista todas as ordens de serviço.
    /// </summary>
    [HttpGet]
    public async Task<Ok<IEnumerable<OrdemServicoResponse>>> ObterTodosAsync(CancellationToken cancellationToken) =>
        TypedResults.Ok((await ordemServicoService.ObterTodosAsync(cancellationToken)).Select(TransformaParaResponse));

    /// <summary>
    /// Obtém uma ordem de serviço pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<OrdemServicoResponse>, NotFound<string>>> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        await ordemServicoService.ObterPorIdAsync(id, cancellationToken) is { } ordemServico
            ? TypedResults.Ok(TransformaParaResponse(ordemServico))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Lista as ordens de serviço de um veículo.
    /// </summary>
    [HttpGet("veiculo/{idVeiculo:int}")]
    public async Task<Ok<IEnumerable<OrdemServicoResponse>>> ObterPorVeiculoAsync(int idVeiculo, CancellationToken cancellationToken) =>
        TypedResults.Ok((await ordemServicoService.ObterPorVeiculoAsync(idVeiculo, cancellationToken)).Select(TransformaParaResponse));

    /// <summary>
    /// Tempo médio de execução (de Em Execução até Finalizada) das OS finalizadas.
    /// Filtra opcionalmente pelo período de finalização (datas inclusivas) e por um serviço incluído na OS.
    /// Retorna 404 quando o serviço informado não existe.
    /// </summary>
    [HttpGet("tempo-medio-execucao")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<Results<Ok<TempoMedioExecucaoResponse>, NotFound<string>>> ObterTempoMedioExecucaoAsync(
        [FromQuery] DateOnly? dataInicio,
        [FromQuery] DateOnly? dataFim,
        [FromQuery] int? idServico,
        CancellationToken cancellationToken) =>
        await ordemServicoService.ObterTempoMedioExecucaoAsync(dataInicio, dataFim, idServico, cancellationToken) is { } tempoMedio
            ? TypedResults.Ok(TransformaParaResponse(tempoMedio))
            : TypedResults.NotFound("Não encontrado");

    /// <summary>
    /// Abre uma OS com status Recebida, registrada em nome do colaborador autenticado.
    /// </summary>
    [HttpPost]
    public async Task<Created<OrdemServicoResponse>> AdicionarAsync(OrdemServicoRequest request, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoService.AdicionarAsync(new OrdemServicoDTO { IdVeiculo = request.IdVeiculo }, cancellationToken);
        return TypedResults.Created(Url.Action(nameof(ObterPorIdAsync), new { id = ordemServico.IdOs }), TransformaParaResponse(ordemServico));
    }

    /// <summary>
    /// Altera o veículo vinculado à OS.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<Results<Ok<OrdemServicoResponse>, NotFound<string>>> AtualizarAsync(int id, OrdemServicoRequest request, CancellationToken cancellationToken) =>
        await ordemServicoService.AtualizarAsync(new OrdemServicoDTO { IdOs = id, IdVeiculo = request.IdVeiculo }, cancellationToken) is { } ordemServico
            ? TypedResults.Ok(TransformaParaResponse(ordemServico))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Histórico de status da OS, com data e colaborador de cada mudança.
    /// </summary>
    [HttpGet("{id:int}/status")]
    public async Task<Results<Ok<IEnumerable<OrdemServicoStatusResponse>>, NotFound<string>>> ObterHistoricoStatusAsync(int id, CancellationToken cancellationToken) =>
        await ordemServicoService.ObterHistoricoStatusAsync(id, cancellationToken) is { } historico
            ? TypedResults.Ok(historico.Select(TransformaParaResponse))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Avança a OS para o próximo status do fluxo, registrado em nome do colaborador autenticado.
    /// </summary>
    [HttpPost("{id:int}/status")]
    public async Task<Results<Ok<OrdemServicoResponse>, NotFound<string>>> AlterarStatusAsync(int id, AlterarStatusRequest request, CancellationToken cancellationToken) =>
        await ordemServicoService.AlterarStatusAsync(id, request.IdStatus, cancellationToken) is { } ordemServico
            ? TypedResults.Ok(TransformaParaResponse(ordemServico))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Lista os serviços incluídos na OS.
    /// </summary>
    [HttpGet("{id:int}/servicos")]
    public async Task<Results<Ok<IEnumerable<OrdemServicoServicoResponse>>, NotFound<string>>> ObterServicosAsync(int id, CancellationToken cancellationToken) =>
        await ordemServicoService.ObterServicosAsync(id, cancellationToken) is { } servicos
            ? TypedResults.Ok(servicos.Select(TransformaParaResponse))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Inclui um serviço na OS com a quantidade informada.
    /// </summary>
    [HttpPost("{id:int}/servicos")]
    public async Task<Results<Created<OrdemServicoServicoResponse>, NotFound<string>>> AdicionarServicoAsync(int id, AdicionarServicoRequest request, CancellationToken cancellationToken) =>
        await ordemServicoService.AdicionarServicoAsync(id, request.IdServico, request.Quantidade, cancellationToken) is { } servico
            ? TypedResults.Created(Url.Action(nameof(ObterServicosAsync), new { id }), TransformaParaResponse(servico))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Altera a quantidade de um serviço da OS.
    /// </summary>
    [HttpPut("{id:int}/servicos/{idOsServico:int}")]
    public async Task<Results<Ok<OrdemServicoServicoResponse>, NotFound<string>>> AlterarQuantidadeServicoAsync(int id, int idOsServico, AlterarQuantidadeServicoRequest request, CancellationToken cancellationToken) =>
        await ordemServicoService.AlterarQuantidadeServicoAsync(id, idOsServico, request.Quantidade, cancellationToken) is { } servico
            ? TypedResults.Ok(TransformaParaResponse(servico))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Remove um serviço da OS.
    /// </summary>
    [HttpDelete("{id:int}/servicos/{idOsServico:int}")]
    public async Task<NoContent> RemoverServicoAsync(int id, int idOsServico, CancellationToken cancellationToken)
    {
        await ordemServicoService.RemoverServicoAsync(id, idOsServico, cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Lista os insumos incluídos na OS.
    /// </summary>
    [HttpGet("{id:int}/insumos")]
    public async Task<Results<Ok<IEnumerable<OrdemServicoInsumoResponse>>, NotFound<string>>> ObterInsumosAsync(int id, CancellationToken cancellationToken) =>
        await ordemServicoService.ObterInsumosAsync(id, cancellationToken) is { } insumos
            ? TypedResults.Ok(insumos.Select(TransformaParaResponse))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Inclui um insumo na OS com a quantidade informada.
    /// </summary>
    [HttpPost("{id:int}/insumos")]
    public async Task<Results<Created<OrdemServicoInsumoResponse>, NotFound<string>>> AdicionarInsumoAsync(int id, AdicionarInsumoRequest request, CancellationToken cancellationToken) =>
        await ordemServicoService.AdicionarInsumoAsync(id, request.IdInsumo, request.Quantidade, cancellationToken) is { } insumo
            ? TypedResults.Created(Url.Action(nameof(ObterInsumosAsync), new { id }), TransformaParaResponse(insumo))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Altera a quantidade de um insumo da OS.
    /// </summary>
    [HttpPut("{id:int}/insumos/{idOsInsumo:int}")]
    public async Task<Results<Ok<OrdemServicoInsumoResponse>, NotFound<string>>> AlterarQuantidadeInsumoAsync(int id, int idOsInsumo, AlterarQuantidadeInsumoRequest request, CancellationToken cancellationToken) =>
        await ordemServicoService.AlterarQuantidadeInsumoAsync(id, idOsInsumo, request.Quantidade, cancellationToken) is { } insumo
            ? TypedResults.Ok(TransformaParaResponse(insumo))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Remove um insumo da OS.
    /// </summary>
    [HttpDelete("{id:int}/insumos/{idOsInsumo:int}")]
    public async Task<NoContent> RemoverInsumoAsync(int id, int idOsInsumo, CancellationToken cancellationToken)
    {
        await ordemServicoService.RemoverInsumoAsync(id, idOsInsumo, cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Remove uma ordem de serviço. Retorna 409 quando o status da OS não permite a remoção.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<Results<NoContent, NotFound<string>, ProblemHttpResult>> DeletarAsync(int id, CancellationToken cancellationToken)
    {
        if (await ordemServicoService.ObterPorIdAsync(id, cancellationToken) is null)
            return TypedResults.NotFound(osNaoEncontrada);

        try
        {
            await ordemServicoService.RemoverAsync(id, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }

    private static OrdemServicoResponse TransformaParaResponse(OrdemServicoDTO dto) => new()
    {
        IdOs = dto.IdOs,
        IdVeiculo = dto.IdVeiculo,
        DataInicio = dto.DataInicio,
        DataFim = dto.DataFim,
        Valor = dto.Valor,
        IdStatus = dto.IdStatus,
        Status = dto.Status
    };

    private static TempoMedioExecucaoResponse TransformaParaResponse(TempoMedioExecucaoDTO dto) => new()
    {
        QuantidadeOrdens = dto.QuantidadeOrdens,
        TempoMedio = dto.TempoMedio,
        TempoMedioEmHoras = dto.TempoMedio is { } tempo ? Math.Round(tempo.TotalHours, 2) : null
    };

    private static OrdemServicoStatusResponse TransformaParaResponse(OrdemServicoStatusDTO dto) => new()
    {
        IdOsStatus = dto.IdOsStatus,
        IdStatus = dto.IdStatus,
        Status = dto.Status,
        IdColaborador = dto.IdColaborador,
        DataInclusao = dto.DataInclusao
    };

    private static OrdemServicoServicoResponse TransformaParaResponse(OrdemServicoServicoDTO dto) => new()
    {
        IdOsServico = dto.IdOsServico,
        IdOs = dto.IdOs,
        IdServico = dto.IdServico,
        Quantidade = dto.Quantidade,
        ValorCobrado = dto.ValorCobrado,
        ValorTotal = dto.ValorTotal
    };

    private static OrdemServicoInsumoResponse TransformaParaResponse(OrdemServicoInsumoDTO dto) => new()
    {
        IdOsInsumo = dto.IdOsInsumo,
        IdOs = dto.IdOs,
        IdInsumo = dto.IdInsumo,
        Quantidade = dto.Quantidade,
        ValorCobrado = dto.ValorCobrado,
        ValorTotal = dto.ValorTotal
    };
}
