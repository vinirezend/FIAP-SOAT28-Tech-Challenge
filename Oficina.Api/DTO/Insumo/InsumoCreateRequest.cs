using System.ComponentModel.DataAnnotations;

namespace Oficina.Api.DTO.Insumo;

public sealed record InsumoCreateRequest
{
    [Required] public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }

    public decimal ValorUnitario { get; init; }

    [Range(0, int.MaxValue)]
    public int QuantidadeEstoque { get; init; }
}
