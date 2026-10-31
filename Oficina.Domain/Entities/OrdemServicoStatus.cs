using Oficina.Domain.Enum;

namespace Oficina.Domain.Entities
{
    /// <summary>
    /// Registro do histórico de status de uma OS (tabela os_status).
    /// </summary>
    public class OrdemServicoStatus
    {
        public int IdOsStatus { get; private set; }
        public int IdOs { get; private set; }
        public StatusOs Status { get; private set; }
        public int IdColaborador { get; private set; }
        public DateTime DataInclusao { get; private set; }

        private OrdemServicoStatus() { }

        public static OrdemServicoStatus Reconstituir(int idOsStatus, int idOs, StatusOs status, int idColaborador, DateTime dataInclusao) =>
        new() { IdOsStatus = idOsStatus, IdOs = idOs, Status = status, IdColaborador = idColaborador, DataInclusao = dataInclusao };
    }
}
