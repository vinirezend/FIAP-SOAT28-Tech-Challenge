using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.Templates;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Application.Implementations.Services;

public class AcompanhamentoOrdemServicoService(
    IOrdemServicoRepository ordemServicoRepository,
    IVeiculoRepository veiculoRepository,
    IClienteRepository clienteRepository,
    IEnvioLinkAssinadoService envioLinkAssinadoService) : IAcompanhamentoOrdemServicoService
{
    public static string CaminhoCliente(int idOs) => $"/api/acompanhamento-ordem-servico/{idOs}/cliente";

    public async Task<AcompanhamentoOrdemServicoDTO?> ObterAsync(int idOs, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ordemServicoRepository.ObterPorIdAsync(idOs, cancellationToken);
        return ordemServico is null ? null : await MontarAcompanhamentoOrdemServicoAsync(ordemServico, cancellationToken);
    }

    public async Task<LinkAssinadoDTO?> EnviarAsync(int idOs, int? minutosValidade, CancellationToken cancellationToken)
    {
        var ordemServico = await ordemServicoRepository.ObterPorIdAsync(idOs, cancellationToken);
        if (ordemServico is null) return null;

        var acompanhamento = await MontarAcompanhamentoOrdemServicoAsync(ordemServico, cancellationToken);
        if (string.IsNullOrWhiteSpace(acompanhamento.ClienteEmail))
            throw new InvalidOperationException("Cliente não possui e-mail cadastrado.");

        return await envioLinkAssinadoService.EnviarAsync(new EnvioLinkAssinadoDTO
        {
            Caminho = CaminhoCliente(idOs),
            EmailDestinatario = acompanhamento.ClienteEmail,
            Assunto = TemplatesEmail.AcompanhamentoOrdemServicoAssunto,
            Template = NomesTemplate.AcompanhamentoOrdemServico.Email,
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

    private async Task<AcompanhamentoOrdemServicoDTO> MontarAcompanhamentoOrdemServicoAsync(OrdemServico ordemServico, CancellationToken cancellationToken)
    {
        var veiculo = await veiculoRepository.ObterPorIdAsync(ordemServico.IdVeiculo, cancellationToken)
            ?? throw new InvalidOperationException("Veículo da OS não encontrado.");
        var cliente = await clienteRepository.ObterPorIdAsync(veiculo.IdCliente, cancellationToken)
            ?? throw new InvalidOperationException("Cliente do veículo não encontrado.");

        var historico = await ordemServicoRepository.ObterHistoricoStatusAsync(ordemServico.IdOs, cancellationToken);
        var etapas = MontarEtapas(ordemServico.Status, historico);

        return new AcompanhamentoOrdemServicoDTO
        {
            IdOs = ordemServico.IdOs,
            IdStatus = (int?)ordemServico.Status,
            Status = ordemServico.Status is { } status ? TemplatesAcompanhamentoOrdemServico.NomeStatus(status) : null,
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
    private static List<AcompanhamentoOrdemServicoEtapaDTO> MontarEtapas(StatusOs? statusAtual, IReadOnlyList<OrdemServicoStatus> historico) =>
        [.. Enum.GetValues<StatusOs>().Select(status =>
        {
            var data = historico.LastOrDefault(h => h.Status == status)?.DataInclusao;
        
            SituacaoEtapa situacao;
            if (status == statusAtual)
                situacao = SituacaoEtapa.Atual;
            else if (statusAtual is null || status > statusAtual)
                situacao = SituacaoEtapa.Pendente;
            else
                situacao = data is null ? SituacaoEtapa.NaoRealizada : SituacaoEtapa.Concluida;
        
            return new AcompanhamentoOrdemServicoEtapaDTO
            {
                IdStatus = (int)status,
                Nome = TemplatesAcompanhamentoOrdemServico.NomeStatus(status),
                Situacao = situacao,
                Data = data
            };
        })];

    private static string? DescreverStatus(StatusOs? statusAtual, List<AcompanhamentoOrdemServicoEtapaDTO> etapas)
    {
        if (statusAtual is null) return null;

        var orcamentoRecusado = statusAtual == StatusOs.Finalizada
            && etapas.Exists(e => e.IdStatus == (int)StatusOs.EmExecucao && e.Situacao == SituacaoEtapa.NaoRealizada);

        return orcamentoRecusado
            ? TemplatesAcompanhamentoOrdemServico.DescricaoOrcamentoRecusado
            : TemplatesAcompanhamentoOrdemServico.DescricaoStatus(statusAtual.Value);
    }
}
