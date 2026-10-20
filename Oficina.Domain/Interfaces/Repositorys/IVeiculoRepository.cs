using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces
{
    public interface IVeiculoRepository
    {
        Task<Veiculo?> ObterPorIdAsync(int idVeiculo);
        Task<IReadOnlyList<Veiculo>> ObterTodosAsync(); 
        Task<Veiculo?> ObterPorPlacaAsync(string placa);       
        Task<IReadOnlyList<Veiculo>?> PesquisarAsync(string? termo);
        Task AdicionarAsync(Veiculo veiculo);
        Task AtualizarAsync(Veiculo veiculo);
        Task RemoverAsync(int idVeiculo);
    }
}