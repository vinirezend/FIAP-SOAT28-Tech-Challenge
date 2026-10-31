namespace Oficina.Api.DTO.Insumo;

public sealed record InsumoQuantidadeResponse
{
    public required string Nome { get; init; }
    public int QuantidadeEstoque { get; init; }
    public int QuantidadeReservada { get; init; }
}
