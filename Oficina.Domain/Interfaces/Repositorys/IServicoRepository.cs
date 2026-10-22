using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IServicoRepository
    {
        Task<Servico?> ObterPorIdAsync(int idServico);
        Task<IReadOnlyList<Servico>> ObterTodosAsync();        
        Task<IReadOnlyList<Servico>?> PesquisarAsync(string? termo);
        Task AdicionarAsync(Servico servico);
        Task AtualizarAsync(Servico servico);
        Task RemoverAsync(int idServico);
    }
}