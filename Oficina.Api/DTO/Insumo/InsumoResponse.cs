namespace Oficina.Api.DTO.Insumo;

public sealed record InsumoResponse
{
    public int IdInsumo { get; init; }
    public required string Nome { get; init; }
    public string? Descricao { get; init; }
    public decimal ValorUnitario { get; init; }
    public bool Ativo { get; init; }
    public int QuantidadeEstoque { get; init; }
    public int QuantidadeReservada { get; init; }
}
