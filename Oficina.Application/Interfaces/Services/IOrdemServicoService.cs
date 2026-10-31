using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IOrdemServicoService
    {
        Task<IReadOnlyList<OrdemServicoDTO>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<OrdemServicoDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OrdemServicoDTO>> ObterPorVeiculoAsync(int idVeiculo, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OrdemServicoStatusDTO>?> ObterHistoricoStatusAsync(int id, CancellationToken cancellationToken = default);
        Task<OrdemServicoDTO> AdicionarAsync(OrdemServicoDTO ordemServico, CancellationToken cancellationToken = default);
        Task<OrdemServicoDTO?> AtualizarAsync(OrdemServicoDTO ordemServico, CancellationToken cancellationToken = default);
        Task<OrdemServicoDTO?> AlterarStatusAsync(int id, int idStatus, CancellationToken cancellationToken = default);
        Task RemoverAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Tempo médio de execução das OS finalizadas no período (datas inclusivas). Retorna nulo quando o serviço informado não existe.</summary>
        Task<TempoMedioExecucaoDTO?> ObterTempoMedioExecucaoAsync(DateOnly? dataInicio, DateOnly? dataFim, int? idServico, CancellationToken cancellationToken = default);

        //Serviços - Operações com serviços da OS
        Task<IReadOnlyList<OrdemServicoServicoDTO>?> ObterServicosAsync(int idOs, CancellationToken cancellationToken = default);
        Task<OrdemServicoServicoDTO?> AdicionarServicoAsync(int idOs, int idServico, int quantidade, CancellationToken cancellationToken = default);
        Task RemoverServicoAsync(int idOs, int idOsServico, CancellationToken cancellationToken = default);
        Task<OrdemServicoServicoDTO?> AlterarQuantidadeServicoAsync(int idOs, int idOsServico, int quantidade, CancellationToken cancellationToken = default);

        //Insumos - Operações com insumos da OS
        Task<IReadOnlyList<OrdemServicoInsumoDTO>?> ObterInsumosAsync(int idOs, CancellationToken cancellationToken = default);
        Task<OrdemServicoInsumoDTO?> AdicionarInsumoAsync(int idOs, int idInsumo, int quantidade, CancellationToken cancellationToken = default);
        Task RemoverInsumoAsync(int idOs, int idOsInsumo, CancellationToken cancellationToken = default);
        Task<OrdemServicoInsumoDTO?> AlterarQuantidadeInsumoAsync(int idOs, int idOsInsumo, int quantidade, CancellationToken cancellationToken = default);
    }
}
