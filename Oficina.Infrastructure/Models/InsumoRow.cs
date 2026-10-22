namespace Oficina.Infrastructure.Models
{
    public sealed record InsumoRow(
        int IdInsumo,
        string Nome,
        string Descricao,
        bool Ativo,
        int QuantidadeEstoque,
        int QuantidadeReservada,
        decimal ValorUnitario
    );    
}