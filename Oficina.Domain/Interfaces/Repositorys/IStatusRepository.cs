using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IStatusRepository
    {
        Task<Status?> ObterPorIdAsync(int idStatus, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Status>> ObterTodosAsync(CancellationToken cancellationToken = default);
    }
}
