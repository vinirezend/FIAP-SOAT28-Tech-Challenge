using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.Orcamento;
using Oficina.Api.Filters;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.Templates;
using Oficina.Domain.Enum;

namespace Oficina.Api.Controllers;

/// <summary>
/// Orçamento da ordem de serviço: consulta e envio pela oficina, aprovação/recusa pelo cliente via link assinado.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrcamentoController(IOrcamentoService orcamentoService, IOrdemServicoService ordemServicoService) : ControllerBase
{
    /// <summary>
    /// Orçamento da OS com cliente, veículo, itens e total.
    /// </summary>
    [HttpGet("{idOs:int}")]
    public async Task<IActionResult> ObterAsync(int idOs)
    {
        try
        {
            var orcamento = await orcamentoService.ObterAsync(idOs);
            if (orcamento is null)
                return NotFound();
            return Ok(TransformaParaResponse(orcamento));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Reenvia o orçamento por e-mail ao cliente, com os botões de aprovar e recusar.
    /// O envio também acontece automaticamente quando a OS passa para Aguardando aprovação.
    /// </summary>
    [HttpPost("{idOs:int}/envio")]
    public async Task<IActionResult> EnviarAsync(int idOs, [FromQuery] int? minutosValidade, CancellationToken cancellationToken)
    {
        try
        {
            var link = await orcamentoService.EnviarAsync(idOs, minutosValidade, cancellationToken);
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
    /// Aberto pelo botão "Aprovar" do e-mail: mostra a confirmação, que faz POST no mesmo link assinado.
    /// </summary>
    [AllowAnonymous]
    [LinkAssinado]
    [HttpGet("{idOs:int}/aprovar")]
    public Task<IActionResult> ConfirmarAprovacaoAsync(int idOs) => PaginaConfirmacaoAsync(idOs, aprovar: true);

    /// <summary>
    /// Aberto pelo botão "Recusar" do e-mail: mostra a confirmação, que faz POST no mesmo link assinado.
    /// </summary>
    [AllowAnonymous]
    [LinkAssinado]
    [HttpGet("{idOs:int}/recusar")]
    public Task<IActionResult> ConfirmarRecusaAsync(int idOs) => PaginaConfirmacaoAsync(idOs, aprovar: false);

    [AllowAnonymous]
    [LinkAssinado]
    [HttpPost("{idOs:int}/aprovar")]
    public async Task<IActionResult> AprovarAsync(int idOs)
    {
        try
        {
            var ordemServico = await orcamentoService.AprovarAsync(idOs);
            if (ordemServico is null)
                return PaginaNaoEncontrado();
            return Html(TemplatesOrcamento.GerarPaginaMensagem("Orçamento aprovado", $"Obrigado! A execução da ordem de serviço nº {idOs} será iniciada."));
        }
        catch (InvalidOperationException ex)
        {
            return Html(TemplatesOrcamento.GerarPaginaMensagem("Não foi possível aprovar o orçamento", ex.Message), StatusCodes.Status400BadRequest);
        }
    }

    [AllowAnonymous]
    [LinkAssinado]
    [HttpPost("{idOs:int}/recusar")]
    public async Task<IActionResult> RecusarAsync(int idOs)
    {
        try
        {
            var ordemServico = await orcamentoService.RecusarAsync(idOs);
            if (ordemServico is null)
                return PaginaNaoEncontrado();
            return Html(TemplatesOrcamento.GerarPaginaMensagem("Orçamento recusado", $"A ordem de serviço nº {idOs} foi finalizada sem execução dos serviços."));
        }
        catch (InvalidOperationException ex)
        {
            return Html(TemplatesOrcamento.GerarPaginaMensagem("Não foi possível recusar o orçamento", ex.Message), StatusCodes.Status400BadRequest);
        }
    }

    private async Task<IActionResult> PaginaConfirmacaoAsync(int idOs, bool aprovar)
    {
        var ordemServico = await ordemServicoService.ObterPorIdAsync(idOs);
        if (ordemServico is null)
            return PaginaNaoEncontrado();

        if (ordemServico.IdStatus != (int)StatusOs.AguardandoAprovacao)
            return Html(TemplatesOrcamento.GerarPaginaMensagem("Orçamento já respondido", $"O orçamento da ordem de serviço nº {idOs} não está mais aguardando aprovação."));

        // POST para a mesma URL, mantendo expira/assinatura da query string
        var urlAcao = $"{Request.PathBase}{Request.Path}{Request.QueryString}";
        return Html(TemplatesOrcamento.GerarPaginaConfirmacao(idOs, aprovar, ordemServico.Valor, urlAcao));
    }

    private static ContentResult PaginaNaoEncontrado() =>
        Html(TemplatesOrcamento.GerarPaginaMensagem("Orçamento não encontrado", "Entre em contato com a oficina."), StatusCodes.Status404NotFound);

    private static ContentResult Html(string conteudo, int statusCode = StatusCodes.Status200OK) => new()
    {
        Content = conteudo,
        ContentType = "text/html; charset=utf-8",
        StatusCode = statusCode
    };

    private static OrcamentoResponse TransformaParaResponse(OrcamentoDTO dto)
    {
        return new OrcamentoResponse
        {
            IdOs = dto.IdOs,
            IdStatus = dto.IdStatus,
            Status = dto.Status,
            ClienteNome = dto.ClienteNome,
            ClienteEmail = dto.ClienteEmail,
            Placa = dto.Placa,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Ano = dto.Ano,
            Servicos = dto.Servicos.Select(TransformaParaResponse),
            Insumos = dto.Insumos.Select(TransformaParaResponse),
            ValorTotal = dto.ValorTotal
        };
    }

    private static OrcamentoItemResponse TransformaParaResponse(OrcamentoItemDTO dto)
    {
        return new OrcamentoItemResponse
        {
            Nome = dto.Nome,
            Quantidade = dto.Quantidade,
            ValorUnitario = dto.ValorUnitario,
            ValorTotal = dto.ValorTotal
        };
    }
}
