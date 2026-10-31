using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.OrdemServico;

public sealed record AdicionarServicoRequest
{
    [JsonRequired, Range(1, int.MaxValue)]
    public int IdServico { get; init; }

    [JsonRequired, Range(1, int.MaxValue)]
    public int Quantidade { get; init; }
}
