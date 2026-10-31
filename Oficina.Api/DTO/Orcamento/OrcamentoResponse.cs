namespace Oficina.Api.DTO.Orcamento;

public sealed record OrcamentoResponse
{
    public int IdOs { get; init; }
    public int? IdStatus { get; init; }
    public string? Status { get; init; }
    public required string ClienteNome { get; init; }
    public required string ClienteEmail { get; init; }
    public required string Placa { get; init; }
    public required string Marca { get; init; }
    public required string Modelo { get; init; }
    public int Ano { get; init; }
    public IEnumerable<OrcamentoItemResponse> Servicos { get; init; } = [];
    public IEnumerable<OrcamentoItemResponse> Insumos { get; init; } = [];
    public decimal ValorTotal { get; init; }
}

public sealed record OrcamentoItemResponse
{
    public required string Nome { get; init; }
    public int Quantidade { get; init; }
    public decimal ValorUnitario { get; init; }
    public decimal ValorTotal { get; init; }
}
