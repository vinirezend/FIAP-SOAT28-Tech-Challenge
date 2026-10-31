namespace Oficina.Api.DTO.AcompanhamentoOrdemServico;

public sealed record AcompanhamentoOrdemServicoResponse
{
    public int IdOs { get; init; }
    public int? IdStatus { get; init; }
    public string? Status { get; init; }
    public string? DescricaoStatus { get; init; }
    public required string ClienteNome { get; init; }
    public required string Placa { get; init; }
    public required string Marca { get; init; }
    public required string Modelo { get; init; }
    public int Ano { get; init; }
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public decimal ValorTotal { get; init; }
    public IEnumerable<AcompanhamentoOrdemServicoEtapaResponse> Etapas { get; init; } = [];
}

public sealed record AcompanhamentoOrdemServicoEtapaResponse
{
    public int IdStatus { get; init; }
    public required string Nome { get; init; }

    /// <summary>Pendente, Atual, Concluida ou NaoRealizada.</summary>
    public required string Situacao { get; init; }
    public DateTime? Data { get; init; }
}
