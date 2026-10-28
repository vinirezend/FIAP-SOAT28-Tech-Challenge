using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IOrdemServicoRepository
    {
        Task<OrdemServico?> ObterPorIdAsync(int idOs);
        Task<IReadOnlyList<OrdemServico>> ObterTodosAsync();
        Task<IReadOnlyList<OrdemServico>> ObterPorVeiculoAsync(int idVeiculo);
        Task<IReadOnlyList<OrdemServicoStatus>> ObterHistoricoStatusAsync(int idOs);

        /// <summary>Insere a OS e o seu primeiro status na mesma transação.</summary>
        Task<OrdemServico> AdicionarAsync(OrdemServico ordemServico, int idColaborador, DateTime dataInclusao);
        Task AtualizarAsync(OrdemServico ordemServico);

        /// <summary>Grava as datas da OS e registra o status atual no histórico na mesma transação.</summary>
        Task AlterarStatusAsync(OrdemServico ordemServico, int idColaborador, DateTime dataInclusao);

        /// <summary>Remove a OS com seu histórico de status e colaboradores.</summary>
        Task RemoverAsync(int idOs);

        Task<IReadOnlyList<OrdemServicoServico>> ObterServicosAsync(int idOs);

        /// <summary>Inclui o serviço na OS e recalcula o valor total na mesma transação.</summary>
        Task<OrdemServicoServico> AdicionarServicoAsync(OrdemServicoServico servico);

        /// <summary>Remove o serviço da OS e recalcula o valor total na mesma transação.</summary>
        Task RemoverServicoAsync(OrdemServicoServico servico);

        /// <summary>Atualiza quantidade/valor cobrado do serviço e recalcula o valor total na mesma transação.</summary>
        Task AtualizarQuantidadeServicoAsync(OrdemServicoServico servico);

        Task<IReadOnlyList<OrdemServicoInsumo>> ObterInsumosAsync(int idOs);

        /// <summary>Inclui o insumo na OS, reserva a quantidade no estoque e recalcula o valor total, tudo na mesma transação.</summary>
        Task<OrdemServicoInsumo> AdicionarInsumoAsync(OrdemServicoInsumo insumo);

        /// <summary>Remove o insumo da OS, libera a quantidade reservada no estoque e recalcula o valor total, tudo na mesma transação.</summary>
        Task RemoverInsumoAsync(OrdemServicoInsumo insumo);

        /// <summary>Atualiza quantidade/valor cobrado do insumo, ajusta a reserva de estoque pela diferença e recalcula o valor total, tudo na mesma transação.</summary>
        Task AtualizarQuantidadeInsumoAsync(OrdemServicoInsumo insumo, int quantidadeAnterior);
    }
}
