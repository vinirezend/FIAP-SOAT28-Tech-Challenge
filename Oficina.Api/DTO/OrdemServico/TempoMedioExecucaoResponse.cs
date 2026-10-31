namespace Oficina.Api.DTO.OrdemServico;

public sealed record TempoMedioExecucaoResponse
{
    /// <summary>Quantidade de OS consideradas no cálculo.</summary>
    public int QuantidadeOrdens { get; init; }

    /// <summary>Tempo médio de execução (d.hh:mm:ss). Nulo quando nenhuma OS atende aos filtros.</summary>
    public TimeSpan? TempoMedio { get; init; }

    /// <summary>Tempo médio de execução em horas, arredondado em 2 casas.</summary>
    public double? TempoMedioEmHoras { get; init; }
}
