using Oficina.Application.DTOs;
namespace Oficina.Application.Interfaces.Services
{
    public interface IServicoService
    {
        Task<ServicoDTO> AdicionarAsync(ServicoDTO servico);
        Task<ServicoDTO?> AtualizarAsync(ServicoDTO servico);
        Task<ServicoDTO?> ObterPorIdAsync(int idServico);
        Task<IReadOnlyList<ServicoDTO>> ObterTodosAsync();
        Task<IReadOnlyList<ServicoDTO>?> PesquisarAsync(string? termo);
        Task RemoverAsync(int idServico);
    }
}