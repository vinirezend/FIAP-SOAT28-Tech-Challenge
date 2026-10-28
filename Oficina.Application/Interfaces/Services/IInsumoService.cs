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
        Task<int> ObterQuantidadeEstoqueAsync(int idInsumo);
        Task<int> ObterQuantidadeDisponivelAsync(int idInsumo);
        Task<int> ObterQuantidadeReservadaAsync(int idInsumo);
        Task<InsumoQuantidadeDTO> AdicionarEstoqueAsync(int idInsumo, int quantidade);
        Task<InsumoQuantidadeDTO> RetirarEstoqueAsync(int idInsumo, int quantidade);
        Task<InsumoQuantidadeDTO> AdicionarReservaAsync(int idInsumo, int quantidade);
        Task<InsumoQuantidadeDTO> RetirarReservaAsync(int idInsumo, int quantidade);
    }
}