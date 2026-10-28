namespace Oficina.Infrastructure.Models
{
    public sealed record OrdemServicoStatusRow(
        int IdOsStatus,
        int IdOs,
        int IdStatus,
        int IdColaborador,
        DateTime DataInclusao
    );
}
