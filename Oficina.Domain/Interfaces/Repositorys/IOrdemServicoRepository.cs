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
    }
}
