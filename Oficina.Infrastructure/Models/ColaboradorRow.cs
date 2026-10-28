namespace Oficina.Infrastructure.Models
{
    public sealed record ColaboradorRow(
        int IdColaborador,
        string Nome,
        string Cpf,
        string Email,
        bool Ativo,
        int IdDepartamento,
        int? IdUsuario
    );
}
