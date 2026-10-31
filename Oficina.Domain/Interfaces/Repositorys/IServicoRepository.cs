using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IServicoRepository
    {
        Task<Servico?> ObterPorIdAsync(int idServico, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Servico>> ObterTodosAsync(CancellationToken cancellationToken = default);        
        Task<IReadOnlyList<Servico>?> PesquisarAsync(string? termo, CancellationToken cancellationToken = default);
        Task<Servico> AdicionarAsync(Servico servico, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Servico servico, CancellationToken cancellationToken = default);
        Task RemoverAsync(int idServico, CancellationToken cancellationToken = default);
    }
}