namespace Oficina.Infrastructure.Models
{
    public sealed record ServicoRow
    (
        int IdServico,
        string Nome,
        string Descricao,
        bool Ativo,
        decimal ValorEstipulado
    );
}