namespace Oficina.Infrastructure.Models
{
    public sealed record TempoMedioExecucaoRow(
        int QuantidadeOrdens,
        double? TempoMedioSegundos
    );
}
