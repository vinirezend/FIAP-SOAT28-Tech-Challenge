namespace Oficina.Infrastructure.Models
{
    public sealed record ServicoRow
    (
        int IdServico,
        string Nome,
        string Descricao,
        decimal ValorEstipulado,
        bool Ativo
    );
}