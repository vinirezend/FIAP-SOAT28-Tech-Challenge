using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.Servico;

public sealed record ServicoUpdateRequest
{
    [Required] public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }

    [JsonRequired]
    public decimal ValorEstipulado { get; init; }

    [JsonRequired]
    public bool Ativo { get; init; }
}
