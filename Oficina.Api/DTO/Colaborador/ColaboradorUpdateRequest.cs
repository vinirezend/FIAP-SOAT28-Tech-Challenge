using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.Colaborador;

public sealed record ColaboradorUpdateRequest
{
    [Required] public required string Nome { get; init; }
    [Required] public required string Email { get; init; }

    [JsonRequired, Range(1, int.MaxValue)]
    public int IdDepartamento { get; init; }

    public int? IdUsuario { get; init; }

    [JsonRequired]
    public bool Ativo { get; init; }
}
