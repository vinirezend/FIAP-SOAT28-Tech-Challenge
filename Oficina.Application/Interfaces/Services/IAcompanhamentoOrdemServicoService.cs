using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IAcompanhamentoOrdemServicoService
    {
        /// <summary>Andamento da OS com status atual, etapas, cliente e veículo. Retorna null se a OS não existir.</summary>
        Task<AcompanhamentoOrdemServicoDTO?> ObterAsync(int idOs);

        /// <summary>
        /// Envia ao cliente, por e-mail, um link assinado para a página de acompanhamento da ordem de serviço.
        /// Retorna null se a OS não existir.
        /// </summary>
        Task<LinkAssinadoDTO?> EnviarAsync(int idOs, int? minutosValidade, CancellationToken cancellationToken);
    }
}
