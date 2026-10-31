namespace Oficina.Api.DTO.OrdemServico;

public sealed record OrdemServicoResponse
{
    public int IdOs { get; init; }
    public int IdVeiculo { get; init; }
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public decimal Valor { get; init; }
    public int? IdStatus { get; init; }
    public string? Status { get; init; }
}
