namespace Oficina.Api.DTO.OrdemServico;

public sealed record OrdemServicoServicoResponse
{
    public int IdOsServico { get; init; }
    public int IdOs { get; init; }
    public int IdServico { get; init; }
    public int Quantidade { get; init; }
    public decimal ValorCobrado { get; init; }
    public decimal ValorTotal { get; init; }
}
