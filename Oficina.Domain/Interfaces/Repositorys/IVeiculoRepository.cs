using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IVeiculoRepository
    {
        Task<Veiculo?> ObterPorIdAsync(int idVeiculo, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Veiculo>> ObterTodosAsync(CancellationToken cancellationToken = default); 
        Task<Veiculo?> ObterPorPlacaAsync(string placa, CancellationToken cancellationToken = default);       
        Task <Veiculo>AdicionarAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
        Task RemoverAsync(int idVeiculo, CancellationToken cancellationToken = default);
    }
}