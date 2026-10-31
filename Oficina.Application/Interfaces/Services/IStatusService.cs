using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IStatusService
    {
        Task<IReadOnlyList<StatusDTO>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<StatusDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
