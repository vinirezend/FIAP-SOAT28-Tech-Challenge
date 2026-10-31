namespace Oficina.Domain.Models
{
    /// <summary>
    /// Média do tempo de execução (início da execução até a finalização) das OS consideradas.
    /// TempoMedio é nulo quando nenhuma OS atende aos filtros.
    /// </summary>
    public sealed record TempoMedioExecucao(int QuantidadeOrdens, TimeSpan? TempoMedio);
}
