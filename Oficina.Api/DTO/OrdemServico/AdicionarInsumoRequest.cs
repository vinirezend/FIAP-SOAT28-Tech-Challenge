using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.OrdemServico;

public sealed record AdicionarInsumoRequest
{
    [JsonRequired, Range(1, int.MaxValue)]
    public int IdInsumo { get; init; }

    [JsonRequired, Range(1, int.MaxValue)]
    public int Quantidade { get; init; }
}
