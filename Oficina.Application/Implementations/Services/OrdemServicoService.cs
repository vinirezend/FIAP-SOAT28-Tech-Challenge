using Microsoft.Extensions.Logging;
using Oficina.Application.Common;
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
        private readonly IServicoRepository _servicoRepository;
        private readonly IInsumoRepository _insumoRepository;
        private readonly ICurrentUser _currentUser;
        private readonly TimeProvider _timeProvider;
        private readonly IOrcamentoService _orcamentoService;
        private readonly ILogger<OrdemServicoService> _logger;

        public OrdemServicoService(
            IOrdemServicoRepository ordemServicoRepository,
            IServicoRepository servicoRepository,
            IInsumoRepository insumoRepository,
            ICurrentUser currentUser,
            TimeProvider timeProvider,
            IOrcamentoService orcamentoService,
            ILogger<OrdemServicoService> logger)
        {
            _ordemServicoRepository = ordemServicoRepository;
            _servicoRepository = servicoRepository;
            _insumoRepository = insumoRepository;
            _currentUser = currentUser;
            _timeProvider = timeProvider;
            _orcamentoService = orcamentoService;
            _logger = logger;
        }

        public async Task<OrdemServicoDTO> AdicionarAsync(OrdemServicoDTO ordemServico, CancellationToken cancellationToken = default)
        {
            var idColaborador = ObterIdColaboradorLogado();
            var ordemServicoEntity = OrdemServico.Criar(ordemServico.IdVeiculo);

            ordemServicoEntity = await _ordemServicoRepository.AdicionarAsync(ordemServicoEntity, idColaborador, Agora(), cancellationToken);
            return TransformaParaDTO(ordemServicoEntity);
        }

        public async Task<OrdemServicoDTO?> AtualizarAsync(OrdemServicoDTO ordemServico, CancellationToken cancellationToken = default)
        {
            var ordemServicoEntity = await _ordemServicoRepository.ObterPorIdAsync(ordemServico.IdOs, cancellationToken);
            if (ordemServicoEntity is null) return null;

            ordemServicoEntity.AtualizarVeiculo(ordemServico.IdVeiculo);

            await _ordemServicoRepository.AtualizarAsync(ordemServicoEntity, cancellationToken);
            return TransformaParaDTO(ordemServicoEntity);
        }

        public async Task<OrdemServicoDTO?> AlterarStatusAsync(int id, int idStatus, CancellationToken cancellationToken = default)
        {
            var idColaborador = ObterIdColaboradorLogado();
            var ordemServicoEntity = await _ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordemServicoEntity is null) return null;

            if ((StatusOs)idStatus == StatusOs.AguardandoAprovacao)
            {
                var valorFinal = await DefinirValorFinalAsync(id, cancellationToken);
                ordemServicoEntity.AtualizarValor(valorFinal);
            }

            var dataAlteracao = Agora();
            ordemServicoEntity.AlterarStatus((StatusOs)idStatus, dataAlteracao);

            await _ordemServicoRepository.AlterarStatusAsync(ordemServicoEntity, idColaborador, dataAlteracao, cancellationToken);

            if (ordemServicoEntity.Status == StatusOs.AguardandoAprovacao)
                await EnviarOrcamentoAsync(id);

            return TransformaParaDTO(ordemServicoEntity);
        }

        /// <summary>
        /// O status já foi gravado: uma falha ao montar o orçamento não desfaz a transição.
        /// O envio pode ser refeito pelo endpoint de reenvio do orçamento.
        /// </summary>
        private async Task EnviarOrcamentoAsync(int id)
        {
            try
            {
                await _orcamentoService.EnviarAsync(id, null, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // Ex.: veículo/cliente não encontrado. Mantém a OS em Aguardando aprovação.
                _logger.LogError(ex, "Falha ao enviar o orçamento da OS {IdOs}. Use o reenvio do orçamento.", id);
            }
        }

        public async Task<IReadOnlyList<OrdemServicoStatusDTO>?> ObterHistoricoStatusAsync(int id, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordemServico is null) return null;

            var historico = await _ordemServicoRepository.ObterHistoricoStatusAsync(id, cancellationToken);
            return historico.Select(TransformaParaDTO).ToList();
        }

        public async Task<OrdemServicoDTO?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
            return ordemServico is null ? null : TransformaParaDTO(ordemServico);
        }

        public async Task<IReadOnlyList<OrdemServicoDTO>> ObterPorVeiculoAsync(int idVeiculo, CancellationToken cancellationToken = default)
        {
            var ordensServico = await _ordemServicoRepository.ObterPorVeiculoAsync(idVeiculo, cancellationToken);
            return ordensServico.Select(TransformaParaDTO).ToList();
        }

        public async Task<IReadOnlyList<OrdemServicoDTO>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var ordensServico = await _ordemServicoRepository.ObterTodosAsync(cancellationToken);
            return ordensServico.Select(TransformaParaDTO).ToList();
        }

        public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
            if (ordemServico is null) return;

            ordemServico.ValidarRemocao();

            var insumos = await _ordemServicoRepository.ObterInsumosAsync(id, cancellationToken);
            foreach (var item in insumos)
                await _insumoRepository.RetirarReservaAsync(item.IdInsumo, item.Quantidade, cancellationToken);

            await _ordemServicoRepository.RemoverAsync(id, cancellationToken);
        }

        public async Task<TempoMedioExecucaoDTO?> ObterTempoMedioExecucaoAsync(DateOnly? dataInicio, DateOnly? dataFim, int? idServico, CancellationToken cancellationToken = default)
        {
            if (dataInicio > dataFim)
                throw new ArgumentException("A data inicial do período não pode ser maior que a data final.");

            if (idServico is not null && await _servicoRepository.ObterPorIdAsync(idServico.Value, cancellationToken) is null)
                return null;

            // Os dias do período são no horário de Brasília; a data final é inclusiva (até o fim desse dia).
            DateTime? finalizadaAPartirDe = dataInicio is { } inicio ? HorarioBrasilia.InicioDoDiaEmUtc(inicio) : null;
            DateTime? finalizadaAntesDe = dataFim is { } fim ? HorarioBrasilia.InicioDoDiaEmUtc(fim.AddDays(1)) : null;

            var tempoMedio = await _ordemServicoRepository.ObterTempoMedioExecucaoAsync(finalizadaAPartirDe, finalizadaAntesDe, idServico, cancellationToken);

            return new TempoMedioExecucaoDTO
            {
                QuantidadeOrdens = tempoMedio.QuantidadeOrdens,
                TempoMedio = tempoMedio.TempoMedio
            };
        }

        public async Task<IReadOnlyList<OrdemServicoServicoDTO>?> ObterServicosAsync(int idOs, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs, cancellationToken);
            if (ordemServico is null) return null;

            var itens = await _ordemServicoRepository.ObterServicosAsync(idOs, cancellationToken);
            return itens.Select(TransformaParaDTO).ToList();
        }

        public async Task<OrdemServicoServicoDTO?> AdicionarServicoAsync(int idOs, int idServico, int quantidade, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs, cancellationToken);
            if (ordemServico is null) return null;

            ordemServico.ValidarAlteracaoItens();

            var servico = await _servicoRepository.ObterPorIdAsync(idServico, cancellationToken);
            if (servico is null || !servico.Ativo)
                throw new InvalidOperationException("Serviço não encontrado ou inativo.");

            var item = OrdemServicoServico.Criar(idOs, idServico, quantidade, servico.ValorEstipulado);
            item = await _ordemServicoRepository.AdicionarServicoAsync(item, cancellationToken);

            return TransformaParaDTO(item);
        }

        public async Task RemoverServicoAsync(int idOs, int idOsServico, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs, cancellationToken);
            if (ordemServico is null) return;

            ordemServico.ValidarAlteracaoItens();

            var itens = await _ordemServicoRepository.ObterServicosAsync(idOs, cancellationToken);
            var item = itens.FirstOrDefault(i => i.IdOsServico == idOsServico);
            if (item is null) return;

            await _ordemServicoRepository.RemoverServicoAsync(item, cancellationToken);
        }

        public async Task<OrdemServicoServicoDTO?> AlterarQuantidadeServicoAsync(int idOs, int idOsServico, int quantidade, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs, cancellationToken);
            if (ordemServico is null) return null;

            ordemServico.ValidarAlteracaoItens();

            var itens = await _ordemServicoRepository.ObterServicosAsync(idOs, cancellationToken);
            var item = itens.FirstOrDefault(i => i.IdOsServico == idOsServico);
            if (item is null) return null;

            var servico = await _servicoRepository.ObterPorIdAsync(item.IdServico, cancellationToken);
            if (servico is null || !servico.Ativo)
                throw new InvalidOperationException("Serviço não encontrado ou inativo.");

            item.AtualizarQuantidade(quantidade);
            item.AtualizarValorCobrado(servico.ValorEstipulado);

            await _ordemServicoRepository.AtualizarQuantidadeServicoAsync(item, cancellationToken);

            return TransformaParaDTO(item);
        }

        public async Task<IReadOnlyList<OrdemServicoInsumoDTO>?> ObterInsumosAsync(int idOs, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs, cancellationToken);
            if (ordemServico is null) return null;

            var itens = await _ordemServicoRepository.ObterInsumosAsync(idOs, cancellationToken);
            return itens.Select(TransformaParaDTO).ToList();
        }

        public async Task<OrdemServicoInsumoDTO?> AdicionarInsumoAsync(int idOs, int idInsumo, int quantidade, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs, cancellationToken);
            if (ordemServico is null) return null;

            ordemServico.ValidarAlteracaoItens();

            var insumo = await _insumoRepository.ObterPorIdAsync(idInsumo, cancellationToken);
            if (insumo is null || !insumo.Ativo)
                throw new InvalidOperationException("Insumo não encontrado ou inativo.");

            await _insumoRepository.AdicionarReservaAsync(idInsumo, quantidade, cancellationToken);

            var item = OrdemServicoInsumo.Criar(idOs, idInsumo, quantidade, insumo.ValorUnitario);
            item = await _ordemServicoRepository.AdicionarInsumoAsync(item, cancellationToken);

            return TransformaParaDTO(item);
        }

        public async Task RemoverInsumoAsync(int idOs, int idOsInsumo, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs, cancellationToken);
            if (ordemServico is null) return;

            ordemServico.ValidarAlteracaoItens();

            var itens = await _ordemServicoRepository.ObterInsumosAsync(idOs, cancellationToken);
            var item = itens.FirstOrDefault(i => i.IdOsInsumo == idOsInsumo);
            if (item is null) return;

            await _insumoRepository.RetirarReservaAsync(item.IdInsumo, item.Quantidade, cancellationToken);
            await _ordemServicoRepository.RemoverInsumoAsync(item, cancellationToken);
        }

        public async Task<OrdemServicoInsumoDTO?> AlterarQuantidadeInsumoAsync(int idOs, int idOsInsumo, int quantidade, CancellationToken cancellationToken = default)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs, cancellationToken);
            if (ordemServico is null) return null;

            ordemServico.ValidarAlteracaoItens();

            var itens = await _ordemServicoRepository.ObterInsumosAsync(idOs, cancellationToken);
            var item = itens.FirstOrDefault(i => i.IdOsInsumo == idOsInsumo);
            if (item is null) return null;

            var quantidadeAnterior = item.Quantidade;

            var insumo = await _insumoRepository.ObterPorIdAsync(item.IdInsumo, cancellationToken);
            if (insumo is null || !insumo.Ativo)
                throw new InvalidOperationException("Insumo não encontrado ou inativo.");

            var delta = quantidade - quantidadeAnterior;
            if (delta > 0)
                await _insumoRepository.AdicionarReservaAsync(item.IdInsumo, delta, cancellationToken);
            else if (delta < 0)
                await _insumoRepository.RetirarReservaAsync(item.IdInsumo, -delta, cancellationToken);

            item.AtualizarQuantidade(quantidade);
            item.AtualizarValorCobrado(insumo.ValorUnitario);

            await _ordemServicoRepository.AtualizarQuantidadeInsumoAsync(item, cancellationToken);

            return TransformaParaDTO(item);
        }

        //VALIDAR SE PRECISA SER OTIMIZADO NO MVP
        // Atualiza item por item (uma transação cada) por simplicidade; se o número de itens por OS crescer,
        // dá pra otimizar fazendo todos os updates numa única transação e recalculando os.valor só uma vez no final.
        private async Task<decimal> DefinirValorFinalAsync(int idOs, CancellationToken cancellationToken)
        {
            var servicos = await _ordemServicoRepository.ObterServicosAsync(idOs, cancellationToken);
            foreach (var item in servicos)
            {
                var servico = await _servicoRepository.ObterPorIdAsync(item.IdServico, cancellationToken);
                if (servico is null || !servico.Ativo)
                    throw new InvalidOperationException("Serviço não encontrado ou inativo.");

                item.AtualizarValorCobrado(servico.ValorEstipulado);
                await _ordemServicoRepository.AtualizarQuantidadeServicoAsync(item, cancellationToken);
            }

            var insumos = await _ordemServicoRepository.ObterInsumosAsync(idOs, cancellationToken);
            foreach (var item in insumos)
            {
                var insumo = await _insumoRepository.ObterPorIdAsync(item.IdInsumo, cancellationToken);
                if (insumo is null || !insumo.Ativo)
                    throw new InvalidOperationException("Insumo não encontrado ou inativo.");

                item.AtualizarValorCobrado(insumo.ValorUnitario);
                await _ordemServicoRepository.AtualizarQuantidadeInsumoAsync(item, cancellationToken);
            }

            return servicos.Sum(s => s.ValorTotal) + insumos.Sum(i => i.ValorTotal);
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

        private static OrdemServicoServicoDTO TransformaParaDTO(OrdemServicoServico item)
        {
            return new OrdemServicoServicoDTO
            {
                IdOsServico = item.IdOsServico,
                IdOs = item.IdOs,
                IdServico = item.IdServico,
                Quantidade = item.Quantidade,
                ValorCobrado = item.ValorCobrado,
                ValorTotal = item.ValorTotal
            };
        }

        private static OrdemServicoInsumoDTO TransformaParaDTO(OrdemServicoInsumo item)
        {
            return new OrdemServicoInsumoDTO
            {
                IdOsInsumo = item.IdOsInsumo,
                IdOs = item.IdOs,
                IdInsumo = item.IdInsumo,
                Quantidade = item.Quantidade,
                ValorCobrado = item.ValorCobrado,
                ValorTotal = item.ValorTotal
            };
        }
    }
}
