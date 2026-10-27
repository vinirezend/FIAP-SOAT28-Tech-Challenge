namespace Oficina.Infrastructure.Models
{
    public sealed record ClienteRow(
        int IdCliente,
        string Nome,
        string NumeroDocumento,
        int TipoDocumento,
        string Email,
        string Telefone
    );
}