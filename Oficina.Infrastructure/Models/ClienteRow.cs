namespace Oficina.Infrastructure.Models
{
    public sealed record ClienteRow(
        int IdCliente,
        string Nome,
        string NumeroDocumento,
        string TipoDocumento,
        string Email,
        string Telefone
    );
}