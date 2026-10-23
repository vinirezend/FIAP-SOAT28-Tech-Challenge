namespace Oficina.Infrastructure.Models
{
    public sealed record VeiculoRow(
        int IdVeiculo,
        int IdCliente,
        string Placa,
        string Modelo,
        string Marca,
        int Ano,
        int IdTipoVeiculo
    );
}