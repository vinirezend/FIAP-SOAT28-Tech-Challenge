using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IColaboradorService
    {
        Task<IReadOnlyList<ColaboradorDTO>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<ColaboradorDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ColaboradorDTO> AdicionarAsync(ColaboradorDTO colaborador, CancellationToken cancellationToken = default);
        Task<ColaboradorDTO?> AtualizarAsync(ColaboradorDTO colaborador, CancellationToken cancellationToken = default);
        Task RemoverAsync(int id, CancellationToken cancellationToken = default);
    }
}
