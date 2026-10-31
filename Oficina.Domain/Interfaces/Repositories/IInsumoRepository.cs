using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces.Repositories
{
    public interface IInsumoRepository
    {
        Task<Insumo?> ObterPorIdAsync(int idInsumo, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Insumo>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Insumo>?> PesquisarAsync(string? termo, CancellationToken cancellationToken = default);
        Task <Insumo>AdicionarAsync(Insumo insumo, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Insumo insumo, CancellationToken cancellationToken = default);
        Task RemoverAsync(int idInsumo, CancellationToken cancellationToken = default);
        Task AdicionarEstoqueAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default);
        Task RetirarEstoqueAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default);
        Task AdicionarReservaAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default);
        Task RetirarReservaAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default);
        Task<int> ObterQuantidadeEstoqueAsync(int idInsumo, CancellationToken cancellationToken = default);
        Task<int> ObterQuantidadeDisponivelAsync(int idInsumo, CancellationToken cancellationToken = default);
        Task<int> ObterQuantidadeReservadaAsync(int idInsumo, CancellationToken cancellationToken = default);
    }
}