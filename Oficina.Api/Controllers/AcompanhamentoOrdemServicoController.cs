using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.AcompanhamentoOrdemServico;
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
    /// <summary>
    /// Status atual da OS e etapas do fluxo, com as datas em que cada uma foi alcançada.
    /// </summary>
    [HttpGet("{idOs:int}")]
    public async Task<IActionResult> ObterAsync(int idOs)
    {
        try
        {
            var acompanhamento = await acompanhamentoOrdemServicoService.ObterAsync(idOs);
            if (acompanhamento is null)
                return NotFound();
            return Ok(TransformaParaResponse(acompanhamento));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Envia ao cliente, por e-mail, um link assinado para acompanhar a OS sem login.
    /// </summary>
    [HttpPost("{idOs:int}/envio")]
    public async Task<IActionResult> EnviarAsync(int idOs, [FromQuery] int? minutosValidade, CancellationToken cancellationToken)
    {
        try
        {
            var link = await acompanhamentoOrdemServicoService.EnviarAsync(idOs, minutosValidade, cancellationToken);
            if (link is null)
                return NotFound();
            return Ok(link);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Página aberta pelo cliente a partir do link do e-mail, com o andamento atualizado a cada acesso.
    /// </summary>
    [AllowAnonymous]
    [LinkAssinado]
    [HttpGet("{idOs:int}/cliente")]
    public async Task<IActionResult> VisualizarAsync(int idOs)
    {
        try
        {
            var acompanhamento = await acompanhamentoOrdemServicoService.ObterAsync(idOs);
            if (acompanhamento is null)
                return Html(TemplatesAcompanhamentoOrdemServico.GerarPaginaMensagem("Ordem de serviço não encontrada", "Entre em contato com a oficina."), StatusCodes.Status404NotFound);
            return Html(TemplatesAcompanhamentoOrdemServico.GerarPagina(acompanhamento));
        }
        catch (InvalidOperationException)
        {
            return Html(TemplatesAcompanhamentoOrdemServico.GerarPaginaMensagem("Não foi possível carregar o andamento", "Entre em contato com a oficina."), StatusCodes.Status400BadRequest);
        }
    }

    private static ContentResult Html(string conteudo, int statusCode = StatusCodes.Status200OK) => new()
    {
        Content = conteudo,
        ContentType = "text/html; charset=utf-8",
        StatusCode = statusCode
    };

    private static AcompanhamentoOrdemServicoResponse TransformaParaResponse(AcompanhamentoOrdemServicoDTO dto) =>
        new()
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

    private static AcompanhamentoOrdemServicoEtapaResponse TransformaParaResponse(AcompanhamentoOrdemServicoEtapaDTO dto) =>
        new()
        {
            IdStatus = dto.IdStatus,
            Nome = dto.Nome,
            Situacao = dto.Situacao.ToString(),
            Data = dto.Data
        };
}
