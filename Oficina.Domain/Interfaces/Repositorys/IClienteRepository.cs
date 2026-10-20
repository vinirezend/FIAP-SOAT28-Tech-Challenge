using Oficina.Domain.Entitys;

namespace Oficina.Domain.Interfaces
{
    public interface IClienteRepository
    {
        Task<Cliente?> ObterPorIdAsync(int idCliente);
        Task<Cliente?> ObterPorDocumentoAsync(string documento);
        Task<IReadOnlyList<Cliente>> ObterTodosAsync();
        Task AdicionarAsync(Cliente cliente);
        Task AtualizarAsync(Cliente cliente);
        Task RemoverAsync(int idCliente);
    }
}