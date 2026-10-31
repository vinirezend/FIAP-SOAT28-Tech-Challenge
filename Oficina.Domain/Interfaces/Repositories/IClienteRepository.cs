using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces.Repositories
{
    public interface IClienteRepository
    {
        Task<Cliente?> ObterPorIdAsync(int idCliente, CancellationToken cancellationToken = default);
        Task<Cliente?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Cliente>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<Cliente> AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
        Task RemoverAsync(int idCliente, CancellationToken cancellationToken = default);
    }
}