using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Implementations.Services;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.Templates;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Application.Services;

public class AcompanhamentoOrdemServicoServiceTests
{
    private static readonly DateTime Recebida = new(2026, 10, 27, 12, 0, 0);
    private static readonly DateTime Diagnostico = new(2026, 10, 28, 13, 0, 0);
    private static readonly DateTime Aguardando = new(2026, 10, 29, 14, 0, 0);

    private readonly Mock<IOrdemServicoRepository> _ordemServicoRepositoryMock = new();
    private readonly Mock<IVeiculoRepository> _veiculoRepositoryMock = new();
    private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
    private readonly Mock<IEnvioLinkAssinadoService> _envioLinkAssinadoServiceMock = new();
    private readonly AcompanhamentoOrdemServicoService _service;

    private EnvioLinkAssinadoDTO? _envio;

    public AcompanhamentoOrdemServicoServiceTests()
    {
        _veiculoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(3))
            .ReturnsAsync(Veiculo.Reconstituir(3, 9, Placa.Criar("FFF2026"), "Gol", "Volkswagen", 2019, TipoVeiculo.Carro));
        _clienteRepositoryMock
            .Setup(r => r.ObterPorIdAsync(9))
            .ReturnsAsync(Cliente.Reconstituir(9, "Lucas Ferreira", Documento.Criar("32887790056"), Email.Criar("lucas@example.com"), Telefone.Criar("(19) 98888-8888")));

        _envioLinkAssinadoServiceMock
            .Setup(s => s.EnviarAsync(It.IsAny<EnvioLinkAssinadoDTO>(), It.IsAny<CancellationToken>()))
            .Callback<EnvioLinkAssinadoDTO, CancellationToken>((envio, _) => _envio = envio)
            .ReturnsAsync(new LinkAssinadoDTO { Url = "https://api.oficina.test/api/acompanhamentoordemservico/1/cliente?expira=1&assinatura=abc" });

        _service = new AcompanhamentoOrdemServicoService(
            _ordemServicoRepositoryMock.Object,
            _veiculoRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _envioLinkAssinadoServiceMock.Object);
    }

    private void ConfigurarOs(StatusOs? status, params OrdemServicoStatus[] historico)
    {
        _ordemServicoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(OrdemServico.Reconstituir(1, 3, null, null, 331.6m, status));
        _ordemServicoRepositoryMock
            .Setup(r => r.ObterHistoricoStatusAsync(1))
            .ReturnsAsync(historico);
    }

    private static OrdemServicoStatus Historico(int id, StatusOs status, DateTime data) =>
        OrdemServicoStatus.Reconstituir(id, 1, status, 2, data);

    private static AcompanhamentoOrdemServicoEtapaDTO Etapa(AcompanhamentoOrdemServicoDTO acompanhamento, StatusOs status) =>
        acompanhamento.Etapas.Single(e => e.IdStatus == (int)status);

    [Fact]
    public async Task ObterAsync_AguardandoAprovacao_DeveMarcarEtapasConcluidasAtualEPendentes()
    {
        ConfigurarOs(StatusOs.AguardandoAprovacao,
            Historico(1, StatusOs.Recebida, Recebida),
            Historico(2, StatusOs.EmDiagnostico, Diagnostico),
            Historico(3, StatusOs.AguardandoAprovacao, Aguardando));

        var acompanhamento = await _service.ObterAsync(1);

        Assert.NotNull(acompanhamento);
        Assert.Equal((int)StatusOs.AguardandoAprovacao, acompanhamento.IdStatus);
        Assert.Equal("Aguardando aprovação", acompanhamento.Status);
        Assert.Equal(TemplatesAcompanhamentoOrdemServico.DescricaoStatus(StatusOs.AguardandoAprovacao), acompanhamento.DescricaoStatus);
        Assert.Equal("Lucas Ferreira", acompanhamento.ClienteNome);
        Assert.Equal("FFF2026", acompanhamento.Placa);
        Assert.Equal(331.6m, acompanhamento.ValorTotal);

        Assert.Equal(6, acompanhamento.Etapas.Count);
        Assert.Equal(SituacaoEtapa.Concluida, Etapa(acompanhamento, StatusOs.Recebida).Situacao);
        Assert.Equal(Recebida, Etapa(acompanhamento, StatusOs.Recebida).Data);
        Assert.Equal(SituacaoEtapa.Concluida, Etapa(acompanhamento, StatusOs.EmDiagnostico).Situacao);
        Assert.Equal(SituacaoEtapa.Atual, Etapa(acompanhamento, StatusOs.AguardandoAprovacao).Situacao);
        Assert.Equal(Aguardando, Etapa(acompanhamento, StatusOs.AguardandoAprovacao).Data);
        Assert.Equal(SituacaoEtapa.Pendente, Etapa(acompanhamento, StatusOs.EmExecucao).Situacao);
        Assert.Null(Etapa(acompanhamento, StatusOs.EmExecucao).Data);
        Assert.Equal(SituacaoEtapa.Pendente, Etapa(acompanhamento, StatusOs.Entregue).Situacao);
    }

    [Fact]
    public async Task ObterAsync_OrcamentoRecusado_DeveMarcarExecucaoComoNaoRealizada()
    {
        ConfigurarOs(StatusOs.Finalizada,
            Historico(1, StatusOs.Recebida, Recebida),
            Historico(2, StatusOs.EmDiagnostico, Diagnostico),
            Historico(3, StatusOs.AguardandoAprovacao, Aguardando),
            Historico(4, StatusOs.Finalizada, Aguardando.AddHours(1)));

        var acompanhamento = await _service.ObterAsync(1);

        Assert.NotNull(acompanhamento);
        Assert.Equal(SituacaoEtapa.NaoRealizada, Etapa(acompanhamento, StatusOs.EmExecucao).Situacao);
        Assert.Equal(SituacaoEtapa.Atual, Etapa(acompanhamento, StatusOs.Finalizada).Situacao);
        Assert.Equal(TemplatesAcompanhamentoOrdemServico.DescricaoOrcamentoRecusado, acompanhamento.DescricaoStatus);
    }

    [Fact]
    public async Task ObterAsync_OrcamentoAprovadoEFinalizado_DeveUsarDescricaoPadrao()
    {
        ConfigurarOs(StatusOs.Finalizada,
            Historico(1, StatusOs.Recebida, Recebida),
            Historico(2, StatusOs.EmDiagnostico, Diagnostico),
            Historico(3, StatusOs.AguardandoAprovacao, Aguardando),
            Historico(4, StatusOs.EmExecucao, Aguardando.AddHours(1)),
            Historico(5, StatusOs.Finalizada, Aguardando.AddHours(5)));

        var acompanhamento = await _service.ObterAsync(1);

        Assert.NotNull(acompanhamento);
        Assert.Equal(SituacaoEtapa.Concluida, Etapa(acompanhamento, StatusOs.EmExecucao).Situacao);
        Assert.Equal(TemplatesAcompanhamentoOrdemServico.DescricaoStatus(StatusOs.Finalizada), acompanhamento.DescricaoStatus);
    }

    [Fact]
    public async Task ObterAsync_OsInexistente_DeveRetornarNull()
    {
        Assert.Null(await _service.ObterAsync(99));
    }

    [Fact]
    public async Task ObterAsync_VeiculoNaoEncontrado_DeveLancarInvalidOperationException()
    {
        _ordemServicoRepositoryMock
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(OrdemServico.Reconstituir(1, 77, null, null, 0m, StatusOs.Recebida));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ObterAsync(1));
    }

    [Fact]
    public async Task EnviarAsync_DeveEnviarEmailComLinkAssinadoDaPaginaDoCliente()
    {
        ConfigurarOs(StatusOs.EmDiagnostico,
            Historico(1, StatusOs.Recebida, Recebida),
            Historico(2, StatusOs.EmDiagnostico, Diagnostico));

        var resultado = await _service.EnviarAsync(1, 120, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.NotNull(_envio);
        Assert.Equal("/api/acompanhamentoordemservico/1/cliente", _envio.Caminho);
        Assert.Equal("lucas@example.com", _envio.EmailDestinatario);
        Assert.Equal(TemplatesEmail.AcompanhamentoOrdemServicoAssunto, _envio.Assunto);
        Assert.Equal(NomesTemplate.AcompanhamentoOrdemServico.Email, _envio.Template);
        Assert.Equal(120, _envio.MinutosValidade);
        Assert.Equal("Lucas Ferreira", _envio.Parametros["nome"]);
        Assert.Equal("1", _envio.Parametros["idOs"]);
        Assert.Equal("Volkswagen Gol (FFF2026)", _envio.Parametros["veiculo"]);
        Assert.Equal("Em diagnóstico", _envio.Parametros["status"]);
    }

    [Fact]
    public async Task EnviarAsync_OsInexistente_DeveRetornarNullSemEnviar()
    {
        Assert.Null(await _service.EnviarAsync(99, null, CancellationToken.None));

        _envioLinkAssinadoServiceMock.Verify(
            s => s.EnviarAsync(It.IsAny<EnvioLinkAssinadoDTO>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
