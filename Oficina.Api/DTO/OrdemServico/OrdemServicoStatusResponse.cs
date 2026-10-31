namespace Oficina.Api.DTO.OrdemServico;

public sealed record OrdemServicoStatusResponse
{
    public int IdOsStatus { get; init; }
    public int IdStatus { get; init; }
    public required string Status { get; init; }
    public int IdColaborador { get; init; }
    public DateTime DataInclusao { get; init; }
}
