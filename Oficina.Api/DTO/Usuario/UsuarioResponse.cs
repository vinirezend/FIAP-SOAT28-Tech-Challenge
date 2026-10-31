namespace Oficina.Api.DTO.Usuario;

/// <summary>
/// Dados públicos do usuário. A senha nunca é devolvida pela API.
/// </summary>
public sealed record UsuarioResponse
{
    public int IdUsuario { get; init; }
    public required string Login { get; init; }
    public bool Ativo { get; init; }
}
