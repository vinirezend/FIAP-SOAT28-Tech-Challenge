using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.Insumo;

public sealed record InsumoUpdateRequest
{
    [Required] public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }

    [JsonRequired]
    public decimal ValorUnitario { get; init; }

    [JsonRequired]
    public bool Ativo { get; init; }

    [JsonRequired, Range(0, int.MaxValue)]
    public int QuantidadeEstoque { get; init; }

    [JsonRequired, Range(0, int.MaxValue)]
    public int QuantidadeReservada { get; init; }
}
