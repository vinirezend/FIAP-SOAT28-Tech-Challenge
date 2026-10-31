namespace Oficina.Api.DTO.OrdemServico;

public sealed record OrdemServicoInsumoResponse
{
    public int IdOsInsumo { get; init; }
    public int IdOs { get; init; }
    public int IdInsumo { get; init; }
    public int Quantidade { get; init; }
    public decimal ValorCobrado { get; init; }
    public decimal ValorTotal { get; init; }
}
