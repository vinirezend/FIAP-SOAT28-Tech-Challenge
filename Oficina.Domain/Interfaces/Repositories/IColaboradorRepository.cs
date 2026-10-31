using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces.Repositories
{
    public interface IColaboradorRepository
    {
        Task<Colaborador?> ObterPorIdAsync(int idColaborador, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Colaborador>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<Colaborador> AdicionarAsync(Colaborador colaborador, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Colaborador colaborador, CancellationToken cancellationToken = default);
        Task RemoverAsync(int idColaborador, CancellationToken cancellationToken = default);
    }
}
