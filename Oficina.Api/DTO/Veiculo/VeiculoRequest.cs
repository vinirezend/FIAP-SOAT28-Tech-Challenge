using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.Veiculo;

public sealed record VeiculoRequest
{
    [Required] public string Placa { get; init; } = string.Empty;
    [Required] public string Modelo { get; init; } = string.Empty;
    [Required] public string Marca { get; init; } = string.Empty;

    [JsonRequired, Range(1, int.MaxValue)]
    public int Ano { get; init; }

    [JsonRequired, Range(1, int.MaxValue)]
    public int IdCliente { get; init; }

    [JsonRequired, Range(1, int.MaxValue)]
    public int IdTipoVeiculo { get; init; }
}
