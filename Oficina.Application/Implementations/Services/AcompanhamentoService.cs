using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.Templates;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Application.Implementations.Services
{
    public class AcompanhamentoService : IAcompanhamentoService
    {
        private readonly IOrdemServicoRepository _ordemServicoRepository;
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IEnvioLinkAssinadoService _envioLinkAssinadoService;

        public AcompanhamentoService(
            IOrdemServicoRepository ordemServicoRepository,
            IVeiculoRepository veiculoRepository,
            IClienteRepository clienteRepository,
            IEnvioLinkAssinadoService envioLinkAssinadoService)
        {
            _ordemServicoRepository = ordemServicoRepository;
            _veiculoRepository = veiculoRepository;
            _clienteRepository = clienteRepository;
            _envioLinkAssinadoService = envioLinkAssinadoService;
        }

        public static string CaminhoCliente(int idOs) => $"/api/acompanhamento/{idOs}/cliente";

        public async Task<AcompanhamentoDTO?> ObterAsync(int idOs)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs);
            return ordemServico is null ? null : await MontarAcompanhamentoAsync(ordemServico);
        }

        public async Task<LinkAssinadoDTO?> EnviarAsync(int idOs, int? minutosValidade, CancellationToken cancellationToken)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs);
            if (ordemServico is null) return null;

            var acompanhamento = await MontarAcompanhamentoAsync(ordemServico);
            if (string.IsNullOrWhiteSpace(acompanhamento.ClienteEmail))
                throw new InvalidOperationException("Cliente não possui e-mail cadastrado.");

            return await _envioLinkAssinadoService.EnviarAsync(new EnvioLinkAssinadoDTO
            {
                Caminho = CaminhoCliente(idOs),
                EmailDestinatario = acompanhamento.ClienteEmail,
                Assunto = TemplatesEmail.AcompanhamentoAssunto,
                TemplateHtml = TemplatesEmail.AcompanhamentoHtml,
                Parametros = new Dictionary<string, string>
                {
                    ["nome"] = acompanhamento.ClienteNome,
                    ["idOs"] = acompanhamento.IdOs.ToString(),
                    ["veiculo"] = $"{acompanhamento.Marca} {acompanhamento.Modelo} ({acompanhamento.Placa})",
                    ["status"] = acompanhamento.Status ?? "Sem status"
                },
                MinutosValidade = minutosValidade
            }, cancellationToken);
        }

        private async Task<AcompanhamentoDTO> MontarAcompanhamentoAsync(OrdemServico ordemServico)
        {
            var veiculo = await _veiculoRepository.ObterPorIdAsync(ordemServico.IdVeiculo)
                ?? throw new InvalidOperationException("Veículo da OS não encontrado.");
            var cliente = await _clienteRepository.ObterPorIdAsync(veiculo.IdCliente)
                ?? throw new InvalidOperationException("Cliente do veículo não encontrado.");

            var historico = await _ordemServicoRepository.ObterHistoricoStatusAsync(ordemServico.IdOs);
            var etapas = MontarEtapas(ordemServico.Status, historico);

            return new AcompanhamentoDTO
            {
                IdOs = ordemServico.IdOs,
                IdStatus = (int?)ordemServico.Status,
                Status = ordemServico.Status is { } status ? TemplatesAcompanhamento.NomeStatus(status) : null,
                DescricaoStatus = DescreverStatus(ordemServico.Status, etapas),
                ClienteNome = cliente.Nome,
                ClienteEmail = cliente.Email.Endereco,
                Placa = veiculo.Placa.Numero,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                Ano = veiculo.Ano,
                DataInicio = ordemServico.DataInicio,
                DataFim = ordemServico.DataFim,
                ValorTotal = ordemServico.Valor,
                Etapas = etapas
            };
        }

        /// <summary>
        /// Uma etapa por status do fluxo. Etapa anterior ao status atual sem registro no
        /// histórico foi pulada (ex.: Em execução quando o orçamento é recusado).
        /// </summary>
        private static List<AcompanhamentoEtapaDTO> MontarEtapas(StatusOs? statusAtual, IReadOnlyList<OrdemServicoStatus> historico)
        {
            return System.Enum.GetValues<StatusOs>().Select(status =>
            {
                var data = historico.LastOrDefault(h => h.Status == status)?.DataInclusao;

                SituacaoEtapa situacao;
                if (status == statusAtual)
                    situacao = SituacaoEtapa.Atual;
                else if (statusAtual is null || status > statusAtual)
                    situacao = SituacaoEtapa.Pendente;
                else
                    situacao = data is null ? SituacaoEtapa.NaoRealizada : SituacaoEtapa.Concluida;

                return new AcompanhamentoEtapaDTO
                {
                    IdStatus = (int)status,
                    Nome = TemplatesAcompanhamento.NomeStatus(status),
                    Situacao = situacao,
                    Data = data
                };
            }).ToList();
        }

        private static string? DescreverStatus(StatusOs? statusAtual, List<AcompanhamentoEtapaDTO> etapas)
        {
            if (statusAtual is null) return null;

            var orcamentoRecusado = statusAtual == StatusOs.Finalizada
                && etapas.Exists(e => e.IdStatus == (int)StatusOs.EmExecucao && e.Situacao == SituacaoEtapa.NaoRealizada);

            return orcamentoRecusado
                ? TemplatesAcompanhamento.DescricaoOrcamentoRecusado
                : TemplatesAcompanhamento.DescricaoStatus(statusAtual.Value);
        }
    }
}
