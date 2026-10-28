using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IColaboradorService
    {
        Task<IReadOnlyList<ColaboradorDTO>> ObterTodosAsync();
        Task<ColaboradorDTO?> ObterPorIdAsync(int id);
        Task<ColaboradorDTO> AdicionarAsync(ColaboradorDTO colaborador);
        Task<ColaboradorDTO?> AtualizarAsync(ColaboradorDTO colaborador);
        Task RemoverAsync(int id);
    }
}
