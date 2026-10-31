using Oficina.Application.DTOs;
namespace Oficina.Application.Interfaces.Services
{
    public interface IServicoService
    {
        Task<ServicoDTO> AdicionarAsync(ServicoDTO servico, CancellationToken cancellationToken = default);
        Task<ServicoDTO?> AtualizarAsync(ServicoDTO servico, CancellationToken cancellationToken = default);
        Task<ServicoDTO?> ObterPorIdAsync(int idServico, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ServicoDTO>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ServicoDTO>> PesquisarAsync(string? termo, CancellationToken cancellationToken = default);
        Task RemoverAsync(int idServico, CancellationToken cancellationToken = default);
    }
}