using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.AcompanhamentoOrdemServico;
using Oficina.Api.ErrorHandling;
using Oficina.Api.Filters;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.Templates;

namespace Oficina.Api.Controllers;

/// <summary>
/// Andamento da ordem de serviço: consulta e envio do link pela oficina, visualização pelo cliente via link assinado.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AcompanhamentoOrdemServicoController(IAcompanhamentoOrdemServicoService acompanhamentoOrdemServicoService) : ControllerBase
{
    private const string osNaoEncontrada = "Ordem de serviço não encontrada.";

    /// <summary>
    /// Status atual da OS e etapas do fluxo, com as datas em que cada uma foi alcançada.
    /// </summary>
    [HttpGet("{idOs:int}")]
    public async Task<Results<Ok<AcompanhamentoOrdemServicoResponse>, NotFound<string>>> ObterAsync(int idOs, CancellationToken cancellationToken) =>
        await acompanhamentoOrdemServicoService.ObterAsync(idOs, cancellationToken) is { } acompanhamento
            ? TypedResults.Ok(TransformaParaResponse(acompanhamento))
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Envia ao cliente, por e-mail, um link assinado para acompanhar a OS sem login.
    /// </summary>
    [HttpPost("{idOs:int}/envio")]
    public async Task<Results<Ok<LinkAssinadoDTO>, NotFound<string>>> EnviarAsync(int idOs, [FromQuery] int? minutosValidade, CancellationToken cancellationToken) =>
        await acompanhamentoOrdemServicoService.EnviarAsync(idOs, minutosValidade, cancellationToken) is { } link
            ? TypedResults.Ok(link)
            : TypedResults.NotFound(osNaoEncontrada);

    /// <summary>
    /// Página aberta pelo cliente a partir do link do e-mail, com o andamento atualizado a cada acesso.
    /// </summary>
    [AllowAnonymous]
    [LinkAssinado]
    [HttpGet("{idOs:int}/cliente")]
    [Produces("text/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ContentResult> VisualizarAsync(int idOs, CancellationToken cancellationToken)
    {
        try
        {
            return await acompanhamentoOrdemServicoService.ObterAsync(idOs, cancellationToken) is { } acompanhamento
                ? PaginaHtml.Criar(TemplatesAcompanhamentoOrdemServico.GerarPagina(acompanhamento))
                : PaginaHtml.Criar(TemplatesAcompanhamentoOrdemServico.GerarPaginaMensagem("Ordem de serviço não encontrada", "Entre em contato com a oficina."), StatusCodes.Status404NotFound);
        }
        catch (InvalidOperationException)
        {
            return PaginaHtml.Criar(TemplatesAcompanhamentoOrdemServico.GerarPaginaMensagem("Não foi possível carregar o andamento", "Entre em contato com a oficina."), StatusCodes.Status400BadRequest);
        }
    }

    private static AcompanhamentoOrdemServicoResponse TransformaParaResponse(AcompanhamentoOrdemServicoDTO dto) => new()
    {
        IdOs = dto.IdOs,
        IdStatus = dto.IdStatus,
        Status = dto.Status,
        DescricaoStatus = dto.DescricaoStatus,
        ClienteNome = dto.ClienteNome,
        Placa = dto.Placa,
        Marca = dto.Marca,
        Modelo = dto.Modelo,
        Ano = dto.Ano,
        DataInicio = dto.DataInicio,
        DataFim = dto.DataFim,
        ValorTotal = dto.ValorTotal,
        Etapas = dto.Etapas.Select(TransformaParaResponse)
    };

    private static AcompanhamentoOrdemServicoEtapaResponse TransformaParaResponse(AcompanhamentoOrdemServicoEtapaDTO dto) => new()
    {
        IdStatus = dto.IdStatus,
        Nome = dto.Nome,
        Situacao = dto.Situacao.ToString(),
        Data = dto.Data
    };
}
