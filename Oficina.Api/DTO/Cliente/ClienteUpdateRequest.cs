using System.ComponentModel.DataAnnotations;

namespace Oficina.Api.DTO.Cliente;

public sealed record ClienteUpdateRequest
{
    [Required] public string Nome { get; init; } = string.Empty;
    [Required] public string Email { get; init; } = string.Empty;
    [Required] public string Telefone { get; init; } = string.Empty;
}
