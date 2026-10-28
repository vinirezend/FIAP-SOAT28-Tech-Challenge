namespace Oficina.Infrastructure.Models
{
    public sealed record OsInsumoRow
    (
        int IdOsInsumo,
        int IdOs,
        int IdInsumo,
        int Quantidade,
        decimal ValorCobrado
    );
}