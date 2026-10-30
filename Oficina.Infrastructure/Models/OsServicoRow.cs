namespace Oficina.Infrastructure.Models
{
    public sealed record OsServicoRow
    (
        int IdOsServico,
        int IdOs,
        int IdServico,
        int Quantidade,
        decimal ValorCobrado
    );
}