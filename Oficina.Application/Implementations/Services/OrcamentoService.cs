using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.Templates;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Application.Implementations.Services
{
    public class OrcamentoService : IOrcamentoService
    {
        private readonly IOrdemServicoRepository _ordemServicoRepository;
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IInsumoRepository _insumoRepository;
        private readonly IEnvioLinkAssinadoService _envioLinkAssinadoService;
        private readonly TimeProvider _timeProvider;

        public OrcamentoService(
            IOrdemServicoRepository ordemServicoRepository,
            IVeiculoRepository veiculoRepository,
            IClienteRepository clienteRepository,
            IServicoRepository servicoRepository,
            IInsumoRepository insumoRepository,
            IEnvioLinkAssinadoService envioLinkAssinadoService,
            TimeProvider timeProvider)
        {
            _ordemServicoRepository = ordemServicoRepository;
            _veiculoRepository = veiculoRepository;
            _clienteRepository = clienteRepository;
            _servicoRepository = servicoRepository;
            _insumoRepository = insumoRepository;
            _envioLinkAssinadoService = envioLinkAssinadoService;
            _timeProvider = timeProvider;
        }

        public static string CaminhoAprovar(int idOs) => $"/api/orcamento/{idOs}/aprovar";
        public static string CaminhoRecusar(int idOs) => $"/api/orcamento/{idOs}/recusar";

        public async Task<OrcamentoDTO?> ObterAsync(int idOs)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs);
            return ordemServico is null ? null : await MontarOrcamentoAsync(ordemServico);
        }

        public async Task<LinkAssinadoDTO?> EnviarAsync(int idOs, int? minutosValidade, CancellationToken cancellationToken)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs);
            if (ordemServico is null) return null;

            if (ordemServico.Status != StatusOs.AguardandoAprovacao)
                throw new InvalidOperationException("O orçamento só pode ser enviado com a OS em Aguardando aprovação.");

            var orcamento = await MontarOrcamentoAsync(ordemServico);
            if (string.IsNullOrWhiteSpace(orcamento.ClienteEmail))
                throw new InvalidOperationException("Cliente não possui e-mail cadastrado.");

            return await _envioLinkAssinadoService.EnviarAsync(new EnvioLinkAssinadoDTO
            {
                EmailDestinatario = orcamento.ClienteEmail,
                Assunto = TemplatesEmail.OrcamentoAssunto,
                TemplateHtml = TemplatesEmail.OrcamentoHtml,
                Parametros = new Dictionary<string, string>
                {
                    ["nome"] = orcamento.ClienteNome,
                    ["idOs"] = orcamento.IdOs.ToString(),
                    ["veiculo"] = $"{orcamento.Marca} {orcamento.Modelo} ({orcamento.Placa})",
                    ["valorTotal"] = TemplatesOrcamento.FormatarMoeda(orcamento.ValorTotal)
                },
                ParametrosHtml = new Dictionary<string, string>
                {
                    ["itens"] = TemplatesOrcamento.GerarLinhasItens(orcamento)
                },
                LinksAssinados = new Dictionary<string, string>
                {
                    ["aprovar"] = CaminhoAprovar(idOs),
                    ["recusar"] = CaminhoRecusar(idOs)
                },
                MinutosValidade = minutosValidade
            }, cancellationToken);
        }

        public async Task<OrdemServicoDTO?> AprovarAsync(int idOs)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs);
            if (ordemServico is null) return null;

            var dataAprovacao = Agora();
            ordemServico.AprovarOrcamento(dataAprovacao);

            var idColaborador = await ObterColaboradorResponsavelAsync(idOs);
            await _ordemServicoRepository.AlterarStatusAsync(ordemServico, idColaborador, dataAprovacao);
            return TransformaParaDTO(ordemServico);
        }

        public async Task<OrdemServicoDTO?> RecusarAsync(int idOs)
        {
            var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(idOs);
            if (ordemServico is null) return null;

            var dataRecusa = Agora();
            ordemServico.RecusarOrcamento(dataRecusa);

            var idColaborador = await ObterColaboradorResponsavelAsync(idOs);

            var insumos = await _ordemServicoRepository.ObterInsumosAsync(idOs);
            foreach (var item in insumos)
                await _insumoRepository.RetirarReservaAsync(item.IdInsumo, item.Quantidade);

            await _ordemServicoRepository.AlterarStatusAsync(ordemServico, idColaborador, dataRecusa);
            return TransformaParaDTO(ordemServico);
        }

        private async Task<OrcamentoDTO> MontarOrcamentoAsync(OrdemServico ordemServico)
        {
            var veiculo = await _veiculoRepository.ObterPorIdAsync(ordemServico.IdVeiculo)
                ?? throw new InvalidOperationException("Veículo da OS não encontrado.");
            var cliente = await _clienteRepository.ObterPorIdAsync(veiculo.IdCliente)
                ?? throw new InvalidOperationException("Cliente do veículo não encontrado.");

            var servicos = new List<OrcamentoItemDTO>();
            foreach (var item in await _ordemServicoRepository.ObterServicosAsync(ordemServico.IdOs))
            {
                var servico = await _servicoRepository.ObterPorIdAsync(item.IdServico);
                servicos.Add(TransformaParaItem(servico?.Nome, item.Quantidade, item.ValorCobrado, item.ValorTotal));
            }

            var insumos = new List<OrcamentoItemDTO>();
            foreach (var item in await _ordemServicoRepository.ObterInsumosAsync(ordemServico.IdOs))
            {
                var insumo = await _insumoRepository.ObterPorIdAsync(item.IdInsumo);
                insumos.Add(TransformaParaItem(insumo?.Nome, item.Quantidade, item.ValorCobrado, item.ValorTotal));
            }

            return new OrcamentoDTO
            {
                IdOs = ordemServico.IdOs,
                IdStatus = (int?)ordemServico.Status,
                Status = ordemServico.Status?.ToString(),
                ClienteNome = cliente.Nome,
                ClienteEmail = cliente.Email.Endereco,
                Placa = veiculo.Placa.Numero,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                Ano = veiculo.Ano,
                Servicos = servicos,
                Insumos = insumos,
                ValorTotal = ordemServico.Valor
            };
        }

        /// <summary>
        /// O cliente responde sem login, mas o histórico exige colaborador:
        /// registra em nome de quem colocou a OS em Aguardando aprovação.
        /// </summary>
        private async Task<int> ObterColaboradorResponsavelAsync(int idOs)
        {
            var historico = await _ordemServicoRepository.ObterHistoricoStatusAsync(idOs);
            return historico.LastOrDefault(h => h.Status == StatusOs.AguardandoAprovacao)?.IdColaborador
                ?? throw new InvalidOperationException("Histórico da OS não possui registro de Aguardando aprovação.");
        }

        // As colunas são "timestamp without time zone": grava o horário UTC sem Kind.
        private DateTime Agora() => _timeProvider.GetUtcNow().DateTime;

        private static OrcamentoItemDTO TransformaParaItem(string? nome, int quantidade, decimal valorUnitario, decimal valorTotal)
        {
            return new OrcamentoItemDTO
            {
                Nome = nome ?? "Item não encontrado",
                Quantidade = quantidade,
                ValorUnitario = valorUnitario,
                ValorTotal = valorTotal
            };
        }

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
    }
}
