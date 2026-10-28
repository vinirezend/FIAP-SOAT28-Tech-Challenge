using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IStatusService
    {
        Task<IReadOnlyList<StatusDTO>> ObterTodosAsync();
        Task<StatusDTO?> ObterPorIdAsync(int id);
    }
}
