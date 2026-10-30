using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IOrcamentoService
    {
        /// <summary>Monta o orçamento da OS com cliente, veículo e itens. Retorna null se a OS não existir.</summary>
        Task<OrcamentoDTO?> ObterAsync(int idOs);

        /// <summary>
        /// Envia o orçamento por e-mail ao cliente, com botões (links assinados) para aprovar ou recusar.
        /// Exige a OS em Aguardando aprovação. Retorna null se a OS não existir.
        /// </summary>
        Task<LinkAssinadoDTO?> EnviarAsync(int idOs, int? minutosValidade, CancellationToken cancellationToken);

        /// <summary>Cliente aprova o orçamento: a OS vai para Em execução.</summary>
        Task<OrdemServicoDTO?> AprovarAsync(int idOs);

        /// <summary>Cliente recusa o orçamento: libera os insumos reservados e finaliza a OS.</summary>
        Task<OrdemServicoDTO?> RecusarAsync(int idOs);
    }
}
