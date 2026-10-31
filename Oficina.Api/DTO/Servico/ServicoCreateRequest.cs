using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.Servico;

public sealed record ServicoCreateRequest
{
    [Required] public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }

    [JsonRequired]
    public decimal ValorEstipulado { get; init; }
}
