using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.Orcamento;
using Oficina.Api.ErrorHandling;
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
    public async Task<Results<Ok<OrcamentoResponse>, NotFound>> ObterAsync(int idOs, CancellationToken cancellationToken) =>
        await orcamentoService.ObterAsync(idOs, cancellationToken) is { } orcamento
            ? TypedResults.Ok(TransformaParaResponse(orcamento))
            : TypedResults.NotFound();

    /// <summary>
    /// Reenvia o orçamento por e-mail ao cliente, com os botões de aprovar e recusar.
    /// O envio também acontece automaticamente quando a OS passa para Aguardando aprovação.
    /// </summary>
    [HttpPost("{idOs:int}/envio")]
    public async Task<Results<Ok<LinkAssinadoDTO>, NotFound>> EnviarAsync(int idOs, [FromQuery] int? minutosValidade, CancellationToken cancellationToken) =>
        await orcamentoService.EnviarAsync(idOs, minutosValidade, cancellationToken) is { } link
            ? TypedResults.Ok(link)
            : TypedResults.NotFound();

    /// <summary>
    /// Aberto pelo botão "Aprovar" do e-mail: mostra a confirmação, que faz POST no mesmo link assinado.
    /// </summary>
    [AllowAnonymous]
    [LinkAssinado]
    [HttpGet("{idOs:int}/aprovar")]
    [Produces("text/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<ContentResult> ConfirmarAprovacaoAsync(int idOs, CancellationToken cancellationToken) => PaginaConfirmacaoAsync(idOs, aprovar: true, cancellationToken);

    /// <summary>
    /// Aberto pelo botão "Recusar" do e-mail: mostra a confirmação, que faz POST no mesmo link assinado.
    /// </summary>
    [AllowAnonymous]
    [LinkAssinado]
    [HttpGet("{idOs:int}/recusar")]
    [Produces("text/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<ContentResult> ConfirmarRecusaAsync(int idOs, CancellationToken cancellationToken) => PaginaConfirmacaoAsync(idOs, aprovar: false, cancellationToken);

    /// <summary>
    /// Confirma a aprovação do orçamento pelo cliente (via link assinado) e libera a execução da OS.
    /// </summary>
    [AllowAnonymous]
    [LinkAssinado]
    [HttpPost("{idOs:int}/aprovar")]
    [Produces("text/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ContentResult> AprovarAsync(int idOs, CancellationToken cancellationToken)
    {
        try
        {
            return await orcamentoService.AprovarAsync(idOs, cancellationToken) is null
                ? PaginaNaoEncontrado()
                : Pagina("Orçamento aprovado", $"Obrigado! A execução da ordem de serviço nº {idOs} será iniciada.");
        }
        catch (InvalidOperationException ex)
        {
            return Pagina("Não foi possível aprovar o orçamento", ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    /// Confirma a recusa do orçamento pelo cliente (via link assinado) e finaliza a OS sem execução.
    /// </summary>
    [AllowAnonymous]
    [LinkAssinado]
    [HttpPost("{idOs:int}/recusar")]
    [Produces("text/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ContentResult> RecusarAsync(int idOs, CancellationToken cancellationToken)
    {
        try
        {
            return await orcamentoService.RecusarAsync(idOs, cancellationToken) is null
                ? PaginaNaoEncontrado()
                : Pagina("Orçamento recusado", $"A ordem de serviço nº {idOs} foi finalizada sem execução dos serviços.");
        }
        catch (InvalidOperationException ex)
        {
            return Pagina("Não foi possível recusar o orçamento", ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    private async Task<ContentResult> PaginaConfirmacaoAsync(int idOs, bool aprovar, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoService.ObterPorIdAsync(idOs, cancellationToken);
        if (ordemServico is null)
            return PaginaNaoEncontrado();

        if (ordemServico.IdStatus != (int)StatusOs.AguardandoAprovacao)
            return Pagina("Orçamento já respondido", $"O orçamento da ordem de serviço nº {idOs} não está mais aguardando aprovação.");

        // POST para a mesma URL, mantendo expira/assinatura da query string
        var urlAcao = $"{Request.PathBase}{Request.Path}{Request.QueryString}";
        return PaginaHtml.Criar(TemplatesOrcamento.GerarPaginaConfirmacao(idOs, aprovar, ordemServico.Valor, urlAcao));
    }

    private static ContentResult Pagina(string titulo, string mensagem, int statusCode = StatusCodes.Status200OK) =>
        PaginaHtml.Criar(TemplatesOrcamento.GerarPaginaMensagem(titulo, mensagem), statusCode);

    private static ContentResult PaginaNaoEncontrado() =>
        Pagina("Orçamento não encontrado", "Entre em contato com a oficina.", StatusCodes.Status404NotFound);

    private static OrcamentoResponse TransformaParaResponse(OrcamentoDTO dto) => new()
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

    private static OrcamentoItemResponse TransformaParaResponse(OrcamentoItemDTO dto) => new()
    {
        Nome = dto.Nome,
        Quantidade = dto.Quantidade,
        ValorUnitario = dto.ValorUnitario,
        ValorTotal = dto.ValorTotal
    };
}
