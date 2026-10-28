using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces.Services
{
    public interface IOrdemServicoService
    {
        Task<IReadOnlyList<OrdemServicoDTO>> ObterTodosAsync();
        Task<OrdemServicoDTO?> ObterPorIdAsync(int id);
        Task<IReadOnlyList<OrdemServicoDTO>> ObterPorVeiculoAsync(int idVeiculo);
        Task<IReadOnlyList<OrdemServicoStatusDTO>?> ObterHistoricoStatusAsync(int id);
        Task<OrdemServicoDTO> AdicionarAsync(OrdemServicoDTO ordemServico);
        Task<OrdemServicoDTO?> AtualizarAsync(OrdemServicoDTO ordemServico);
        Task<OrdemServicoDTO?> AlterarStatusAsync(int id, int idStatus);
        Task RemoverAsync(int id);
    }
}
