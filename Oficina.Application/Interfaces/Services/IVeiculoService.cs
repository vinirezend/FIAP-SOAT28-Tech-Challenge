using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IVeiculoService
    {
        Task<IEnumerable<VeiculoDTO>> ObterTodosAsync();
        Task<VeiculoDTO?> ObterPorIdAsync(int id);
        Task<VeiculoDTO?> ObterPorPlacaAsync(string placa);
        Task<VeiculoDTO> AdicionarAsync(VeiculoDTO veiculo);
        Task<VeiculoDTO?> AtualizarAsync(VeiculoDTO veiculo);
        Task RemoverAsync(int id);
    }
}