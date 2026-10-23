using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.Entitys;

namespace Oficina.Application.Implementations.Services
{
    public class InsumoService : IInsumoService
    {
        private readonly IInsumoRepository _insumoRepository;
        public InsumoService(IInsumoRepository insumoRepository)
        {
            _insumoRepository = insumoRepository;
        }
        public async Task<InsumoDTO> AdicionarAsync(InsumoDTO insumo)
        {
            var insumoEntity = Insumo.Criar(
                insumo.Nome,
                insumo.Descricao,
                insumo.ValorUnitario,
                insumo.QuantidadeEstoque
            );
            insumoEntity = await _insumoRepository.AdicionarAsync(insumoEntity);
            return TransformaParaDTO(insumoEntity);
        }
        public async Task<InsumoDTO?> AtualizarAsync(InsumoDTO insumo)
        {
            var insumoRecord = await _insumoRepository.ObterPorIdAsync(insumo.IdInsumo);
            if (insumoRecord is null)
                return null;

            insumoRecord.Atualizar(
                insumo.Nome,
                insumo.Descricao,
                insumo.ValorUnitario,
                insumo.Ativo,
                insumo.QuantidadeEstoque,
                insumo.QuantidadeReservada
            );
            await _insumoRepository.AtualizarAsync(insumoRecord);
            return TransformaParaDTO(insumoRecord);
        }
        public async Task<InsumoDTO?> ObterPorIdAsync(int id)
        {
            var insumo = await _insumoRepository.ObterPorIdAsync(id);
            return insumo is null ? null : TransformaParaDTO(insumo);
        }
        public async Task<IReadOnlyList<InsumoDTO>> ObterTodosAsync()
        {
           var insumos = await _insumoRepository.ObterTodosAsync();
           return insumos.Select(TransformaParaDTO).ToList();
        }
        public async Task<IReadOnlyList<InsumoDTO>> PesquisarAsync(string termo)
        {
            var insumos = await _insumoRepository.PesquisarAsync(termo);
            return insumos is null ? Array.Empty<InsumoDTO>() : insumos.Select(TransformaParaDTO).ToList();
        }
        public Task RemoverAsync(int id)
        {
            return _insumoRepository.RemoverAsync(id);
        }
        private static InsumoDTO TransformaParaDTO(Insumo insumo){
            return new InsumoDTO
            {
                IdInsumo = insumo.IdInsumo,
                Nome = insumo.Nome,
                Descricao = insumo.Descricao,
                ValorUnitario = insumo.ValorUnitario,
                Ativo = insumo.Ativo,
                QuantidadeEstoque = insumo.QuantidadeEstoque,
                QuantidadeReservada = insumo.QuantidadeReservada

            };
        }
    }
}