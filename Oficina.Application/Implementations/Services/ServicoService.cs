using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.Entitys;

namespace Oficina.Application.Implementations.Services
{
    public class ServicoService : IServicoService
    {
        private readonly IServicoRepository _servicoRepository;

        public ServicoService(IServicoRepository servicoRepository)
        {
            _servicoRepository = servicoRepository;
        }

        public async Task<ServicoDTO> AdicionarAsync(ServicoDTO servico)
        {
            var servicoEntity = Servico.Criar(
                servico.Nome,
                servico.Descricao,
                servico.ValorEstipulado);
                servicoEntity = await _servicoRepository.AdicionarAsync(servicoEntity);
                return TransformaParaDTO(servicoEntity);
        }

        public async Task<ServicoDTO?> AtualizarAsync(ServicoDTO servico)
        {
            var servicoRecord = await _servicoRepository.ObterPorIdAsync(servico.IdServico);
            if (servicoRecord == null)
                return null;
                
            servicoRecord.Atualizar(
                servico.Nome,
                servico.Descricao,
                servico.ValorEstipulado,
                servico.Ativo);

            await _servicoRepository.AtualizarAsync(servicoRecord);
            return TransformaParaDTO(servicoRecord);
        }

        public async Task<ServicoDTO?> ObterPorIdAsync(int idServico)
        {
            var servico = await _servicoRepository.ObterPorIdAsync(idServico);  
            return servico is null ? null : TransformaParaDTO(servico);
        }

        public async Task<IReadOnlyList<ServicoDTO>> ObterTodosAsync()
        {
            var servicos = await _servicoRepository.ObterTodosAsync();
            return servicos.Select(TransformaParaDTO).ToList(); 
        }

        public async Task<IReadOnlyList<ServicoDTO>> PesquisarAsync(string? termo)
        {
            var servicos = await _servicoRepository.PesquisarAsync(termo);
            return servicos is null ? Array.Empty<ServicoDTO>() : servicos.Select(TransformaParaDTO).ToList();
        }

        public async Task RemoverAsync(int idServico)
        {
            await _servicoRepository.RemoverAsync(idServico);
        }

        private ServicoDTO TransformaParaDTO(Servico servico)
        {
            return new ServicoDTO
            {
                IdServico = servico.IdServico,
                Nome = servico.Nome,
                Descricao = servico.Descricao,
                ValorEstipulado = servico.ValorEstipulado,
                Ativo = servico.Ativo
            };
        }
    }
}