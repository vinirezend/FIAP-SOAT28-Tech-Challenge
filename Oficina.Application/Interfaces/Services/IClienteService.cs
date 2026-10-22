using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<ClienteDTO>> ObterTodosAsync();
        Task<ClienteDTO?> ObterPorIdAsync(int id);
        Task<ClienteDTO> AdicionarAsync(ClienteDTO cliente);
        Task<ClienteDTO?> AtualizarAsync(ClienteDTO cliente);
        Task<ClienteDTO?> ObterPorDocumentoAsync(string documento);
        Task RemoverAsync(int id);

    }
}
