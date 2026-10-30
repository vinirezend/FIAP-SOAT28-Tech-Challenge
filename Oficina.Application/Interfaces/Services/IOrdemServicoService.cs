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
        
        //Serviços - Operações com serviços da OS
        Task<IReadOnlyList<OrdemServicoServicoDTO>?> ObterServicosAsync(int idOs);
        Task<OrdemServicoServicoDTO?> AdicionarServicoAsync(int idOs, int idServico, int quantidade);
        Task RemoverServicoAsync(int idOs, int idOsServico);
        Task<OrdemServicoServicoDTO?> AlterarQuantidadeServicoAsync(int idOs, int idOsServico, int quantidade);

        //Insumos - Operações com insumos da OS
        Task<IReadOnlyList<OrdemServicoInsumoDTO>?> ObterInsumosAsync(int idOs);
        Task<OrdemServicoInsumoDTO?> AdicionarInsumoAsync(int idOs, int idInsumo, int quantidade);
        Task RemoverInsumoAsync(int idOs, int idOsInsumo);
        Task<OrdemServicoInsumoDTO?> AlterarQuantidadeInsumoAsync(int idOs, int idOsInsumo, int quantidade);
    }
}
