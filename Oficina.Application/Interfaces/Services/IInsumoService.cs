using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IInsumoService
    {
        Task<InsumoDTO> AdicionarAsync(InsumoDTO insumo);
        Task<InsumoDTO?> AtualizarAsync(InsumoDTO insumo);
        Task<InsumoDTO?> ObterPorIdAsync(int id); 
        Task<IReadOnlyList<InsumoDTO>> ObterTodosAsync();
        Task<IReadOnlyList<InsumoDTO>> PesquisarAsync(string termo); 
        Task RemoverAsync(int id);
    }
}