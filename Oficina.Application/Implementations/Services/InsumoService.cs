using Oficina.Application.DTOs;
using Oficina.Application.Exceptions;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;

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

        public async Task<int> ObterQuantidadeEstoqueAsync(int idInsumo)
        {
            return await _insumoRepository.ObterQuantidadeEstoqueAsync(idInsumo);
        }

        public async Task<int> ObterQuantidadeDisponivelAsync(int idInsumo)
        {
            return await _insumoRepository.ObterQuantidadeDisponivelAsync(idInsumo);
        }

        public async Task<int> ObterQuantidadeReservadaAsync(int idInsumo)
        {
            return await _insumoRepository.ObterQuantidadeReservadaAsync(idInsumo);
        }

        public async Task<InsumoQuantidadeDTO> AdicionarEstoqueAsync(int idInsumo, int quantidade)
        {
            ValidarQuantidade(quantidade);

            await _insumoRepository.AdicionarEstoqueAsync(idInsumo, quantidade);

            var insumo = await _insumoRepository.ObterPorIdAsync(idInsumo);

            return new InsumoQuantidadeDTO()
            {
                Nome = insumo.Nome,
                QuantidadeEstoque = insumo.QuantidadeEstoque,
                QuantidadeReservada = insumo.QuantidadeReservada
            };
        }

        public async Task<InsumoQuantidadeDTO> RetirarEstoqueAsync(int idInsumo, int quantidade)
        {
            ValidarQuantidade(quantidade);

            await _insumoRepository.RetirarEstoqueAsync(idInsumo, quantidade);

            var insumo = await _insumoRepository.ObterPorIdAsync(idInsumo);

            return new InsumoQuantidadeDTO()
            {
                Nome = insumo.Nome,
                QuantidadeEstoque = insumo.QuantidadeEstoque,
                QuantidadeReservada = insumo.QuantidadeReservada
            };
        }

        public async Task<InsumoQuantidadeDTO> AdicionarReservaAsync(int idInsumo, int quantidade)
        {
            ValidarQuantidade(quantidade);

            await _insumoRepository.AdicionarReservaAsync(idInsumo, quantidade);

            var insumo = await _insumoRepository.ObterPorIdAsync(idInsumo);

            return new InsumoQuantidadeDTO()
            {
                Nome = insumo.Nome,
                QuantidadeEstoque = insumo.QuantidadeEstoque,
                QuantidadeReservada = insumo.QuantidadeReservada
            };
        }

        public async Task<InsumoQuantidadeDTO> RetirarReservaAsync(int idInsumo, int quantidade)
        {
            ValidarQuantidade(quantidade);

            await _insumoRepository.RetirarReservaAsync(idInsumo, quantidade);

            var insumo = await _insumoRepository.ObterPorIdAsync(idInsumo);

            return new InsumoQuantidadeDTO()
            {
                Nome = insumo.Nome,
                QuantidadeEstoque = insumo.QuantidadeEstoque,
                QuantidadeReservada = insumo.QuantidadeReservada
            };
        }

        private static void ValidarQuantidade(int quantidade)
        {
            if (quantidade <= 0)
                throw new BusinessException("A quantidade deve ser maior que zero.");
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