using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.OrdemServico;

public sealed record AlterarStatusRequest
{
    [JsonRequired, Range(1, int.MaxValue)]
    public int IdStatus { get; init; }
}
