using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IOrdemServicoRepository
    {
        Task<OrdemServico?> ObterPorIdAsync(int idOs, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OrdemServico>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OrdemServico>> ObterPorVeiculoAsync(int idVeiculo, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OrdemServicoStatus>> ObterHistoricoStatusAsync(int idOs, CancellationToken cancellationToken = default);

        /// <summary>Insere a OS e o seu primeiro status na mesma transação.</summary>
        Task<OrdemServico> AdicionarAsync(OrdemServico ordemServico, int idColaborador, DateTime dataInclusao, CancellationToken cancellationToken = default);
        Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);

        /// <summary>Grava as datas da OS e registra o status atual no histórico na mesma transação.</summary>
        Task AlterarStatusAsync(OrdemServico ordemServico, int idColaborador, DateTime dataInclusao, CancellationToken cancellationToken = default);

        /// <summary>Remove a OS com seu histórico de status e colaboradores.</summary>
        Task RemoverAsync(int idOs, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OrdemServicoServico>> ObterServicosAsync(int idOs, CancellationToken cancellationToken = default);

        /// <summary>Inclui o serviço na OS e recalcula o valor total na mesma transação.</summary>
        Task<OrdemServicoServico> AdicionarServicoAsync(OrdemServicoServico servico, CancellationToken cancellationToken = default);

        /// <summary>Remove o serviço da OS e recalcula o valor total na mesma transação.</summary>
        Task RemoverServicoAsync(OrdemServicoServico servico, CancellationToken cancellationToken = default);

        /// <summary>Atualiza quantidade/valor cobrado do serviço e recalcula o valor total na mesma transação.</summary>
        Task AtualizarQuantidadeServicoAsync(OrdemServicoServico servico, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OrdemServicoInsumo>> ObterInsumosAsync(int idOs, CancellationToken cancellationToken = default);

        /// <summary>Inclui o insumo na OS, reserva a quantidade no estoque e recalcula o valor total, tudo na mesma transação.</summary>
        Task<OrdemServicoInsumo> AdicionarInsumoAsync(OrdemServicoInsumo insumo, CancellationToken cancellationToken = default);

        /// <summary>Remove o insumo da OS, libera a quantidade reservada no estoque e recalcula o valor total, tudo na mesma transação.</summary>
        Task RemoverInsumoAsync(OrdemServicoInsumo insumo, CancellationToken cancellationToken = default);

        /// <summary>Atualiza quantidade/valor cobrado do insumo e recalcula o valor total, na mesma transação. O ajuste de reserva de estoque é responsabilidade do Service, via IInsumoRepository.</summary>
        Task AtualizarQuantidadeInsumoAsync(OrdemServicoInsumo insumo, CancellationToken cancellationToken = default);
    }
}
