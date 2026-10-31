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
        public async Task<InsumoDTO> AdicionarAsync(InsumoDTO insumo, CancellationToken cancellationToken = default)
        {
            var insumoEntity = Insumo.Criar(
                insumo.Nome,
                insumo.Descricao,
                insumo.ValorUnitario,
                insumo.QuantidadeEstoque
            );
            insumoEntity = await _insumoRepository.AdicionarAsync(insumoEntity, cancellationToken);
            return TransformaParaDTO(insumoEntity);
        }
        public async Task<InsumoDTO?> AtualizarAsync(InsumoDTO insumo, CancellationToken cancellationToken = default)
        {
            var insumoRecord = await _insumoRepository.ObterPorIdAsync(insumo.IdInsumo, cancellationToken);
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
            await _insumoRepository.AtualizarAsync(insumoRecord, cancellationToken);
            return TransformaParaDTO(insumoRecord);
        }
        public async Task<InsumoDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var insumo = await _insumoRepository.ObterPorIdAsync(id, cancellationToken);
            return insumo is null ? null : TransformaParaDTO(insumo);
        }
        public async Task<IReadOnlyList<InsumoDTO>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
           var insumos = await _insumoRepository.ObterTodosAsync(cancellationToken);
           return insumos.Select(TransformaParaDTO).ToList();
        }
        public async Task<IReadOnlyList<InsumoDTO>> PesquisarAsync(string termo, CancellationToken cancellationToken = default)
        {
            var insumos = await _insumoRepository.PesquisarAsync(termo, cancellationToken);
            return insumos is null ? Array.Empty<InsumoDTO>() : insumos.Select(TransformaParaDTO).ToList();
        }
        public Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            return _insumoRepository.RemoverAsync(id, cancellationToken);
        }

        public async Task<int> ObterQuantidadeEstoqueAsync(int idInsumo, CancellationToken cancellationToken = default)
        {
            return await _insumoRepository.ObterQuantidadeEstoqueAsync(idInsumo, cancellationToken);
        }

        public async Task<int> ObterQuantidadeDisponivelAsync(int idInsumo, CancellationToken cancellationToken = default)
        {
            return await _insumoRepository.ObterQuantidadeDisponivelAsync(idInsumo, cancellationToken);
        }

        public async Task<int> ObterQuantidadeReservadaAsync(int idInsumo, CancellationToken cancellationToken = default)
        {
            return await _insumoRepository.ObterQuantidadeReservadaAsync(idInsumo, cancellationToken);
        }

        public async Task<InsumoQuantidadeDTO> AdicionarEstoqueAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default)
        {
            ValidarQuantidade(quantidade);

            await _insumoRepository.AdicionarEstoqueAsync(idInsumo, quantidade, cancellationToken);

            return await ObterQuantidadesAsync(idInsumo, cancellationToken);
        }

        public async Task<InsumoQuantidadeDTO> RetirarEstoqueAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default)
        {
            ValidarQuantidade(quantidade);

            await _insumoRepository.RetirarEstoqueAsync(idInsumo, quantidade, cancellationToken);

            return await ObterQuantidadesAsync(idInsumo, cancellationToken);
        }

        public async Task<InsumoQuantidadeDTO> AdicionarReservaAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default)
        {
            ValidarQuantidade(quantidade);

            await _insumoRepository.AdicionarReservaAsync(idInsumo, quantidade, cancellationToken);

            return await ObterQuantidadesAsync(idInsumo, cancellationToken);
        }

        public async Task<InsumoQuantidadeDTO> RetirarReservaAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default)
        {
            ValidarQuantidade(quantidade);

            await _insumoRepository.RetirarReservaAsync(idInsumo, quantidade, cancellationToken);

            return await ObterQuantidadesAsync(idInsumo, cancellationToken);
        }

        private async Task<InsumoQuantidadeDTO> ObterQuantidadesAsync(int idInsumo, CancellationToken cancellationToken)
        {
            var insumo = await _insumoRepository.ObterPorIdAsync(idInsumo, cancellationToken)
                ?? throw new BusinessException("Insumo não localizado.");

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