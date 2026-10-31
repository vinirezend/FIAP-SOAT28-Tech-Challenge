using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.Veiculo;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;

namespace Oficina.Api.Controllers;

/// <summary>
/// Cadastro de veículos dos clientes.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class VeiculoController(IVeiculoService veiculoService) : ControllerBase
{
    private const string veiculoNaoEncontrado = "Veículo não encontrado.";

    /// <summary>
    /// Lista todos os veículos.
    /// </summary>
    [HttpGet]
    public async Task<Ok<IEnumerable<VeiculoResponse>>> ObterTodosAsync(CancellationToken cancellationToken) =>
        TypedResults.Ok((await veiculoService.ObterTodosAsync(cancellationToken)).Select(TransformaParaResponse));

    /// <summary>
    /// Obtém um veículo pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<VeiculoResponse>, NotFound<string>>> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        await veiculoService.ObterPorIdAsync(id, cancellationToken) is { } veiculo
            ? TypedResults.Ok(TransformaParaResponse(veiculo))
            : TypedResults.NotFound(veiculoNaoEncontrado);

    /// <summary>
    /// Obtém um veículo pela placa.
    /// </summary>
    [HttpGet("placa/{placa}")]
    public async Task<Results<Ok<VeiculoResponse>, NotFound<string>>> ObterPorPlacaAsync(string placa, CancellationToken cancellationToken) =>
        await veiculoService.ObterPorPlacaAsync(placa, cancellationToken) is { } veiculo
            ? TypedResults.Ok(TransformaParaResponse(veiculo))
            : TypedResults.NotFound(veiculoNaoEncontrado);

    /// <summary>
    /// Cadastra um novo veículo vinculado a um cliente.
    /// </summary>
    [HttpPost]
    public async Task<Created<VeiculoResponse>> AdicionarAsync(VeiculoRequest request, CancellationToken cancellationToken)
    {
        var veiculo = await veiculoService.AdicionarAsync(TransformaParaDTO(0, request), cancellationToken);
        return TypedResults.Created(Url.Action(nameof(ObterPorIdAsync), new { id = veiculo.IdVeiculo }), TransformaParaResponse(veiculo));
    }

    /// <summary>
    /// Atualiza os dados de um veículo.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<Results<Ok<VeiculoResponse>, NotFound<string>>> AtualizarAsync(int id, VeiculoRequest request, CancellationToken cancellationToken) =>
        await veiculoService.AtualizarAsync(TransformaParaDTO(id, request), cancellationToken) is { } veiculo
            ? TypedResults.Ok(TransformaParaResponse(veiculo))
            : TypedResults.NotFound(veiculoNaoEncontrado);

    /// <summary>
    /// Remove um veículo.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<Results<NoContent, NotFound<string>>> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        if (await veiculoService.ObterPorIdAsync(id, cancellationToken) is null)
            return TypedResults.NotFound(veiculoNaoEncontrado);

        await veiculoService.RemoverAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }

    private static VeiculoDTO TransformaParaDTO(int id, VeiculoRequest request) => new()
    {
        IdVeiculo = id,
        Placa = request.Placa,
        Modelo = request.Modelo,
        Marca = request.Marca,
        Ano = request.Ano,
        IdCliente = request.IdCliente,
        IdTipoVeiculo = request.IdTipoVeiculo
    };

    private static VeiculoResponse TransformaParaResponse(VeiculoDTO dto) => new()
    {
        IdVeiculo = dto.IdVeiculo,
        Placa = dto.Placa,
        Modelo = dto.Modelo,
        Marca = dto.Marca,
        Ano = dto.Ano,
        IdCliente = dto.IdCliente,
        IdTipoVeiculo = dto.IdTipoVeiculo
    };
}
