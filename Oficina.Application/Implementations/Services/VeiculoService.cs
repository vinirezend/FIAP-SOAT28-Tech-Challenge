using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces.Repositories;
using Oficina.Domain.ValueObjects;
using Oficina.Domain.Enum;

namespace Oficina.Application.Implementations.Services
{
    public class VeiculoService : IVeiculoService
    {
        private readonly IVeiculoRepository _veiculoRepository;
        public VeiculoService(IVeiculoRepository veiculoRepository)
        {
            _veiculoRepository = veiculoRepository;
        }
        public async Task<VeiculoDTO> AdicionarAsync(VeiculoDTO veiculo, CancellationToken cancellationToken = default)
        {
            var veiculoEntity = Veiculo.Criar(
                veiculo.IdCliente,
                Placa.Criar(veiculo.Placa),
                veiculo.Modelo,
                veiculo.Marca,
                veiculo.Ano,
                (TipoVeiculo)veiculo.IdTipoVeiculo
            );
            veiculoEntity = await _veiculoRepository.AdicionarAsync(veiculoEntity, cancellationToken);
            return TransformaParaDTO(veiculoEntity);
        }

        public async Task<VeiculoDTO?> AtualizarAsync(VeiculoDTO veiculo, CancellationToken cancellationToken = default)
        {
            var veiculoEntity = await _veiculoRepository.ObterPorIdAsync(veiculo.IdVeiculo, cancellationToken);
            if (veiculoEntity is null) return null;

            veiculoEntity.Atualizar(
                veiculo.IdCliente,
                Placa.Criar(veiculo.Placa),
                veiculo.Modelo,
                veiculo.Marca,
                veiculo.Ano,
                (TipoVeiculo)veiculo.IdTipoVeiculo
            );

            await _veiculoRepository.AtualizarAsync(veiculoEntity, cancellationToken);
            return TransformaParaDTO(veiculoEntity);
        }

        public async Task<VeiculoDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken);
            return veiculo is null ? null : TransformaParaDTO(veiculo); 
        }

        public async Task<VeiculoDTO?> ObterPorPlacaAsync(string placa, CancellationToken cancellationToken = default)
        {
            var veiculo = await _veiculoRepository.ObterPorPlacaAsync(placa, cancellationToken);
            return veiculo is null ? null : TransformaParaDTO(veiculo);
        }

        public async Task<IReadOnlyList<VeiculoDTO>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var veiculos = await _veiculoRepository.ObterTodosAsync(cancellationToken);
            return veiculos.Select(TransformaParaDTO).ToList();
        }
        public Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            return _veiculoRepository.RemoverAsync(id, cancellationToken);
        }
        private static VeiculoDTO TransformaParaDTO(Veiculo veiculo)
        {
            return new VeiculoDTO
            {
                IdVeiculo = veiculo.IdVeiculo,
                Placa = veiculo.Placa.Numero,
                Modelo = veiculo.Modelo,
                Ano = veiculo.Ano,
                IdCliente = veiculo.IdCliente,
                IdTipoVeiculo = (int)veiculo.IdTipoVeiculo,
                Marca = veiculo.Marca
            };
        }
    }
}