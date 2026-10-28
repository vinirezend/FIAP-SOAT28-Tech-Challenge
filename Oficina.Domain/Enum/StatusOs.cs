namespace Oficina.Domain.Enum
{
    /// <summary>
    /// Espelha os registros da tabela status, na ordem do fluxo da OS.
    /// </summary>
    public enum StatusOs
    {
        Recebida = 1,
        EmDiagnostico = 2,
        AguardandoAprovacao = 3,
        EmExecucao = 4,
        Finalizada = 5,
        Entregue = 6
    }
}
