using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces
{
    public interface IInsumoRepository
    {
        Task<Insumo?> ObterPorIdAsync(int idInsumo);
        Task<IReadOnlyList<Insumo>> ObterTodosAsync();
        Task<IReadOnlyList<Insumo>?> PesquisarAsync(string? termo);
        Task AdicionarAsync(Insumo insumo);
        Task AtualizarAsync(Insumo insumo);
        Task RemoverAsync(int idInsumo); 
    }
}