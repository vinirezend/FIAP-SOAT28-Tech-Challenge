using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Application.Implementations.Services
{
    public class OrdemServicoService : IOrdemServicoService
    {
        private readonly IOrdemServicoRepository _ordemServicoRepository;
        private readonly ICurrentUser _currentUser;
        private readonly TimeProvider _timeProvider;

        public OrdemServicoService(IOrdemServicoRepository ordemServicoRepository, ICurrentUser currentUser, TimeProvider timeProvider)
        {
            _ordemServicoRepository = ordemServicoRepository;
            _currentUser = currentUser;
            _timeProvider = timeProvider;
        }

        public async Task<OrdemServicoDTO> AdicionarAsync(OrdemServicoDTO ordemServico)
        {
            var idColaborador = ObterIdColaboradorLogado();
            var ordemServicoEntity = OrdemServico.Criar(ordemServico.IdVeiculo);

            ordemServicoEntity = await _ordemServicoRepository.AdicionarAsync(ordemServicoEntity, idColaborador, Agora());
            return TransformaParaDTO(ordemServicoEntity);
        }

        public async Task<OrdemServicoDTO?> AtualizarAsync(OrdemServicoDTO ordemServico)
        {
            var ordemServicoEntity = await _ordemServicoRepository.ObterPorIdAsync(ordemServico.IdOs);
            if (ordemServicoEntity is null) return null;

            ordemServicoEntity.AtualizarVeiculo(ordemServico.IdVeiculo);

            await _ordemServicoRepository.AtualizarAsync(ordemServicoEntity);
            return TransformaParaDTO(ordemServicoEntity);
        }

        public async Task<OrdemServicoDTO?> AlterarStatusAsync(int id, int idStatus)
        {
            var idColaborador = ObterIdColaboradorLogado();
            var ordemServicoEntity = await _ordemServicoRepository.ObterPorIdAsync(id);
            if (ordemServicoEntity is null) return null;

            var dataAlteracao = Agora();
            ordemServicoEntity.AlterarStatus((StatusOs)idStatus, dataAlteracao);

            await _ordemServicoRepository.AlterarStatusAsync(ordemServicoEntity, idColaborador, dataAlteracao);
            return TransformaParaDTO(ordemServicoEntity);
        }

        public async Task<IReadOnlyList<OrdemServicoStatusDTO>?> ObterHistoricoStatusAsync(int id)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(id);
            if (ordemServico is null) return null;

            var historico = await _ordemServicoRepository.ObterHistoricoStatusAsync(id);
            return historico.Select(TransformaParaDTO).ToList();
        }

        public async Task<OrdemServicoDTO?> ObterPorIdAsync(int id)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(id);
            return ordemServico is null ? null : TransformaParaDTO(ordemServico);
        }

        public async Task<IReadOnlyList<OrdemServicoDTO>> ObterPorVeiculoAsync(int idVeiculo)
        {
            var ordensServico = await _ordemServicoRepository.ObterPorVeiculoAsync(idVeiculo);
            return ordensServico.Select(TransformaParaDTO).ToList();
        }

        public async Task<IReadOnlyList<OrdemServicoDTO>> ObterTodosAsync()
        {
            var ordensServico = await _ordemServicoRepository.ObterTodosAsync();
            return ordensServico.Select(TransformaParaDTO).ToList();
        }

        public async Task RemoverAsync(int id)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(id);
            if (ordemServico is null) return;

            ordemServico.ValidarRemocao();
            await _ordemServicoRepository.RemoverAsync(id);
        }

        private int ObterIdColaboradorLogado()
        {
            return _currentUser.IdColaborador
                ?? throw new InvalidOperationException("O usuário autenticado não está vinculado a um colaborador.");
        }

        // As colunas são "timestamp without time zone": grava o horário UTC sem Kind.
        private DateTime Agora() => _timeProvider.GetUtcNow().DateTime;

        private static OrdemServicoDTO TransformaParaDTO(OrdemServico ordemServico)
        {
            return new OrdemServicoDTO
            {
                IdOs = ordemServico.IdOs,
                IdVeiculo = ordemServico.IdVeiculo,
                DataInicio = ordemServico.DataInicio,
                DataFim = ordemServico.DataFim,
                Valor = ordemServico.Valor,
                IdStatus = (int?)ordemServico.Status,
                Status = ordemServico.Status?.ToString()
            };
        }

        private static OrdemServicoStatusDTO TransformaParaDTO(OrdemServicoStatus ordemServicoStatus)
        {
            return new OrdemServicoStatusDTO
            {
                IdOsStatus = ordemServicoStatus.IdOsStatus,
                IdOs = ordemServicoStatus.IdOs,
                IdStatus = (int)ordemServicoStatus.Status,
                Status = ordemServicoStatus.Status.ToString(),
                IdColaborador = ordemServicoStatus.IdColaborador,
                DataInclusao = ordemServicoStatus.DataInclusao
            };
        }
    }
}
