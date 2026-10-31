using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Interfaces.Repositories;
using Oficina.Domain.Entities;

namespace Oficina.Application.Implementations.Services
{
    public class ServicoService : IServicoService
    {
        private readonly IServicoRepository _servicoRepository;

        public ServicoService(IServicoRepository servicoRepository)
        {
            _servicoRepository = servicoRepository;
        }

        public async Task<ServicoDTO> AdicionarAsync(ServicoDTO servico, CancellationToken cancellationToken = default)
        {
            var servicoEntity = Servico.Criar(
                servico.Nome,
                servico.Descricao,
                servico.ValorEstipulado);
                servicoEntity = await _servicoRepository.AdicionarAsync(servicoEntity, cancellationToken);
                return TransformaParaDTO(servicoEntity);
        }

        public async Task<ServicoDTO?> AtualizarAsync(ServicoDTO servico, CancellationToken cancellationToken = default)
        {
            var servicoRecord = await _servicoRepository.ObterPorIdAsync(servico.IdServico, cancellationToken);
            if (servicoRecord == null)
                return null;
                
            servicoRecord.Atualizar(
                servico.Nome,
                servico.Descricao,
                servico.ValorEstipulado,
                servico.Ativo);

            await _servicoRepository.AtualizarAsync(servicoRecord, cancellationToken);
            return TransformaParaDTO(servicoRecord);
        }

        public async Task<ServicoDTO?> ObterPorIdAsync(int idServico, CancellationToken cancellationToken = default)
        {
            var servico = await _servicoRepository.ObterPorIdAsync(idServico, cancellationToken);  
            return servico is null ? null : TransformaParaDTO(servico);
        }

        public async Task<IReadOnlyList<ServicoDTO>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var servicos = await _servicoRepository.ObterTodosAsync(cancellationToken);
            return servicos.Select(TransformaParaDTO).ToList(); 
        }

        public async Task<IReadOnlyList<ServicoDTO>> PesquisarAsync(string? termo, CancellationToken cancellationToken = default)
        {
            var servicos = await _servicoRepository.PesquisarAsync(termo, cancellationToken);
            return servicos is null ? Array.Empty<ServicoDTO>() : servicos.Select(TransformaParaDTO).ToList();
        }

        public async Task RemoverAsync(int idServico, CancellationToken cancellationToken = default)
        {
            await _servicoRepository.RemoverAsync(idServico, cancellationToken);
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