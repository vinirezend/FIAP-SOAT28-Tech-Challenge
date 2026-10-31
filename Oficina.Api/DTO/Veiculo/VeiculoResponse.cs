namespace Oficina.Api.DTO.Veiculo;

public sealed record VeiculoResponse
{
    public int IdVeiculo { get; init; }
    public required string Placa { get; init; }
    public required string Modelo { get; init; }
    public required string Marca { get; init; }
    public int Ano { get; init; }
    public int IdCliente { get; init; }
    public int IdTipoVeiculo { get; init; }
}
