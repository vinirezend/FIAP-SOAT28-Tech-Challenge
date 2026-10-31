using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IClienteService
    {
        Task<IReadOnlyList<ClienteDTO>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<ClienteDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ClienteDTO> AdicionarAsync(ClienteDTO cliente, CancellationToken cancellationToken = default);
        Task<ClienteDTO?> AtualizarAsync(ClienteDTO cliente, CancellationToken cancellationToken = default);
        Task<ClienteDTO?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default);
        Task RemoverAsync(int id, CancellationToken cancellationToken = default);

    }
}
