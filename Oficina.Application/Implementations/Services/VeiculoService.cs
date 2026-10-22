using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
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
        public async Task<VeiculoDTO> AdicionarAsync(VeiculoDTO veiculo)
        {
            var veiculoEntity = Veiculo.Criar(
                veiculo.IdCliente,
                Placa.Criar(veiculo.Placa),
                veiculo.Modelo,
                veiculo.Marca,
                veiculo.Ano,
                (TipoVeiculo)veiculo.IdTipoVeiculo
            );
            veiculoEntity = await _veiculoRepository.AdicionarAsync(veiculoEntity);
            return TransformaParaDTO(veiculoEntity);
        }

        public async Task<VeiculoDTO?> AtualizarAsync(VeiculoDTO veiculo)
        {
            var veiculoEntity = await _veiculoRepository.ObterPorIdAsync(veiculo.IdVeiculo);
            if (veiculoEntity is null) return null;

            veiculoEntity.Atualizar(
                veiculo.IdCliente,
                Placa.Criar(veiculo.Placa),
                veiculo.Modelo,
                veiculo.Marca,
                veiculo.Ano,
                (TipoVeiculo)veiculo.IdTipoVeiculo
            );

            await _veiculoRepository.AtualizarAsync(veiculoEntity);
            return TransformaParaDTO(veiculoEntity);
        }

        public async Task<VeiculoDTO?> ObterPorIdAsync(int id)
        {
            var veiculo = await _veiculoRepository.ObterPorIdAsync(id);
            return veiculo is null ? null : TransformaParaDTO(veiculo); 
        }

        public async Task<VeiculoDTO?> ObterPorPlacaAsync(string placa)
        {
            var veiculo = await _veiculoRepository.ObterPorPlacaAsync(placa);
            return veiculo is null ? null : TransformaParaDTO(veiculo);
        }

        public async Task<IReadOnlyList<VeiculoDTO>> ObterTodosAsync()
        {
            var veiculos = await _veiculoRepository.ObterTodosAsync();
            return veiculos.Select(TransformaParaDTO).ToList();
        }
        public Task RemoverAsync(int id)
        {
            return _veiculoRepository.RemoverAsync(id);
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