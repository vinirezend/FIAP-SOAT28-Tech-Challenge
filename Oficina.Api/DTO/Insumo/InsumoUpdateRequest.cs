using System.ComponentModel.DataAnnotations;

namespace Oficina.Api.DTO.Insumo;

public sealed record InsumoUpdateRequest
{
    [Required] public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }

    public decimal ValorUnitario { get; init; }

    public bool Ativo { get; init; }

    [Range(0, int.MaxValue)]
    public int QuantidadeEstoque { get; init; }

    [Range(0, int.MaxValue)]
    public int QuantidadeReservada { get; init; }
}
