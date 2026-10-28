using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IStatusRepository
    {
        Task<Status?> ObterPorIdAsync(int idStatus);
        Task<IReadOnlyList<Status>> ObterTodosAsync();
    }
}
