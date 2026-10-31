namespace Oficina.Api.DTO.Cliente;

public sealed record ClienteResponse
{
    public int IdCliente { get; init; }
    public required string Nome { get; init; }
    public required string Documento { get; init; }
    public required string TipoDocumento { get; init; }
    public required string Email { get; init; }
    public required string Telefone { get; init; }
}
