using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IVeiculoService
    {
        Task<IReadOnlyList<VeiculoDTO>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<VeiculoDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<VeiculoDTO?> ObterPorPlacaAsync(string placa, CancellationToken cancellationToken = default);
        Task<VeiculoDTO> AdicionarAsync(VeiculoDTO veiculo, CancellationToken cancellationToken = default);
        Task<VeiculoDTO?> AtualizarAsync(VeiculoDTO veiculo, CancellationToken cancellationToken = default);
        Task RemoverAsync(int id, CancellationToken cancellationToken = default);
    }
}