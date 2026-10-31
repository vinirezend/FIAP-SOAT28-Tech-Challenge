using System.ComponentModel.DataAnnotations;

namespace Oficina.Api.DTO.Cliente;

public sealed record ClienteCreateRequest
{
    [Required] public string Nome { get; init; } = string.Empty;

    /// <summary>CPF ou CNPJ, com ou sem máscara.</summary>
    [Required] public string Documento { get; init; } = string.Empty;

    [Required] public string Email { get; init; } = string.Empty;
    [Required] public string Telefone { get; init; } = string.Empty;
}
