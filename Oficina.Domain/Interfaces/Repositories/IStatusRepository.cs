using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces.Repositories
{
    public interface IStatusRepository
    {
        Task<Status?> ObterPorIdAsync(int idStatus, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Status>> ObterTodosAsync(CancellationToken cancellationToken = default);
    }
}
