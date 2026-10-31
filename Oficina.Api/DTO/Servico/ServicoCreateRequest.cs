using System.ComponentModel.DataAnnotations;

namespace Oficina.Api.DTO.Servico;

public sealed record ServicoCreateRequest
{
    [Required] public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }

    public decimal ValorEstipulado { get; init; }
}
