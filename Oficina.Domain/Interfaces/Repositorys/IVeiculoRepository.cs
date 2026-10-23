using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IVeiculoRepository
    {
        Task<Veiculo?> ObterPorIdAsync(int idVeiculo);
        Task<IReadOnlyList<Veiculo>> ObterTodosAsync(); 
        Task<Veiculo?> ObterPorPlacaAsync(string placa);       
        Task <Veiculo>AdicionarAsync(Veiculo veiculo);
        Task AtualizarAsync(Veiculo veiculo);
        Task RemoverAsync(int idVeiculo);
    }
}