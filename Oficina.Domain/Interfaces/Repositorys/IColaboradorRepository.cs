using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces.Repositorys
{
    public interface IColaboradorRepository
    {
        Task<Colaborador?> ObterPorIdAsync(int idColaborador);
        Task<IReadOnlyList<Colaborador>> ObterTodosAsync();
        Task<Colaborador> AdicionarAsync(Colaborador colaborador);
        Task AtualizarAsync(Colaborador colaborador);
        Task RemoverAsync(int idColaborador);
    }
}
