using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
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
