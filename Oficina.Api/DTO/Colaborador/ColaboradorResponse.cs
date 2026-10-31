namespace Oficina.Api.DTO.Colaborador;

public sealed record ColaboradorResponse
{
    public int IdColaborador { get; init; }
    public required string Nome { get; init; }
    public required string Cpf { get; init; }
    public required string Email { get; init; }
    public bool Ativo { get; init; }
    public int IdDepartamento { get; init; }
    public int? IdUsuario { get; init; }
}
