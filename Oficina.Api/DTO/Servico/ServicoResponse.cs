namespace Oficina.Api.DTO.Servico;

public sealed record ServicoResponse
{
    public int IdServico { get; init; }
    public required string Nome { get; init; }
    public string? Descricao { get; init; }
    public decimal ValorEstipulado { get; init; }
    public bool Ativo { get; init; }
}
