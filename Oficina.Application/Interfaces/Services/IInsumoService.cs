using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IInsumoService
    {
        Task<InsumoDTO> AdicionarAsync(InsumoDTO insumo, CancellationToken cancellationToken = default);
        Task<InsumoDTO?> AtualizarAsync(InsumoDTO insumo, CancellationToken cancellationToken = default);
        Task<InsumoDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default); 
        Task<IReadOnlyList<InsumoDTO>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<InsumoDTO>> PesquisarAsync(string termo, CancellationToken cancellationToken = default); 
        Task RemoverAsync(int id, CancellationToken cancellationToken = default);
        Task<int> ObterQuantidadeEstoqueAsync(int idInsumo, CancellationToken cancellationToken = default);
        Task<int> ObterQuantidadeDisponivelAsync(int idInsumo, CancellationToken cancellationToken = default);
        Task<int> ObterQuantidadeReservadaAsync(int idInsumo, CancellationToken cancellationToken = default);
        Task<InsumoQuantidadeDTO> AdicionarEstoqueAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default);
        Task<InsumoQuantidadeDTO> RetirarEstoqueAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default);
        Task<InsumoQuantidadeDTO> AdicionarReservaAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default);
        Task<InsumoQuantidadeDTO> RetirarReservaAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default);
    }
}