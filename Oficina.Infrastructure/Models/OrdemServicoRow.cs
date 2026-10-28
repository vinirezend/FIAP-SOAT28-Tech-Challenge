namespace Oficina.Infrastructure.Models
{
    public sealed record OrdemServicoRow(
        int IdOs,
        int IdVeiculo,
        DateTime? DataInicio,
        DateTime? DataFim,
        decimal Valor,
        int? IdStatus
    );
}
