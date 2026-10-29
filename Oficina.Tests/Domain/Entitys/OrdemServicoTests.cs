using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;

namespace Oficina.Tests.Domain.Entitys
{
    public class OrdemServicoTests
    {
        private static readonly DateTime Data = new(2026, 10, 27, 12, 0, 0);

        private static OrdemServico CriarComStatus(StatusOs? status) =>
            OrdemServico.Reconstituir(1, 1, null, null, 0, status);

        [Fact]
        public void CriarOrdemServico_DeveIniciarComoRecebidaSemValor()
        {
            var resultado = OrdemServico.Criar(1);

            Assert.Equal(StatusOs.Recebida, resultado.Status);
            Assert.Equal(0, resultado.Valor);
            Assert.Null(resultado.DataInicio);
            Assert.Null(resultado.DataFim);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CriarOrdemServico_ComVeiculoInvalido_DeveLancarExcecao(int idVeiculo)
        {
            Assert.Throws<ArgumentException>(() => OrdemServico.Criar(idVeiculo));
        }

        [Theory]
        [InlineData(null, StatusOs.Recebida)]
        [InlineData(StatusOs.Recebida, StatusOs.EmDiagnostico)]
        [InlineData(StatusOs.EmDiagnostico, StatusOs.AguardandoAprovacao)]
        [InlineData(StatusOs.AguardandoAprovacao, StatusOs.EmExecucao)]
        [InlineData(StatusOs.EmExecucao, StatusOs.Finalizada)]
        [InlineData(StatusOs.Finalizada, StatusOs.Entregue)]
        public void AlterarStatus_ParaProximoStatus_DeveAvancar(StatusOs? atual, StatusOs novo)
        {
            var ordemServico = CriarComStatus(atual);

            ordemServico.AlterarStatus(novo, Data);

            Assert.Equal(novo, ordemServico.Status);
        }

        [Theory]
        [InlineData(StatusOs.Recebida, StatusOs.EmExecucao)]
        [InlineData(StatusOs.EmDiagnostico, StatusOs.Recebida)]
        [InlineData(StatusOs.EmExecucao, StatusOs.EmExecucao)]
        [InlineData(null, StatusOs.EmDiagnostico)]
        public void AlterarStatus_ForaDoFluxo_DeveLancarExcecao(StatusOs? atual, StatusOs novo)
        {
            var ordemServico = CriarComStatus(atual);

            Assert.Throws<InvalidOperationException>(() => ordemServico.AlterarStatus(novo, Data));
            Assert.Equal(atual, ordemServico.Status);
        }

        [Fact]
        public void AlterarStatus_OsEntregue_DeveLancarExcecao()
        {
            var ordemServico = CriarComStatus(StatusOs.Entregue);

            Assert.Throws<InvalidOperationException>(() => ordemServico.AlterarStatus(StatusOs.Entregue, Data));
        }

        [Fact]
        public void AlterarStatus_StatusInexistente_DeveLancarExcecao()
        {
            var ordemServico = CriarComStatus(StatusOs.Recebida);

            Assert.Throws<ArgumentException>(() => ordemServico.AlterarStatus((StatusOs)99, Data));
        }

        [Fact]
        public void AlterarStatus_ParaEmExecucao_DeveRegistrarDataInicio()
        {
            var ordemServico = CriarComStatus(StatusOs.AguardandoAprovacao);

            ordemServico.AlterarStatus(StatusOs.EmExecucao, Data);

            Assert.Equal(Data, ordemServico.DataInicio);
            Assert.Null(ordemServico.DataFim);
        }

        [Fact]
        public void AlterarStatus_ParaFinalizada_DeveRegistrarDataFim()
        {
            var ordemServico = CriarComStatus(StatusOs.EmExecucao);

            ordemServico.AlterarStatus(StatusOs.Finalizada, Data);

            Assert.Equal(Data, ordemServico.DataFim);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(StatusOs.Recebida)]
        [InlineData(StatusOs.EmDiagnostico)]
        public void AtualizarVeiculo_AteODiagnostico_DeveAlterar(StatusOs? status)
        {
            var ordemServico = CriarComStatus(status);

            ordemServico.AtualizarVeiculo(2);

            Assert.Equal(2, ordemServico.IdVeiculo);
        }

        [Theory]
        [InlineData(StatusOs.AguardandoAprovacao)]
        [InlineData(StatusOs.Entregue)]
        public void AtualizarVeiculo_AposODiagnostico_DeveLancarExcecao(StatusOs status)
        {
            var ordemServico = CriarComStatus(status);

            Assert.Throws<InvalidOperationException>(() => ordemServico.AtualizarVeiculo(2));
        }

        [Fact]
        public void ValidarRemocao_OsRecebida_NaoDeveLancarExcecao()
        {
            var ordemServico = CriarComStatus(StatusOs.Recebida);

            var excecao = Record.Exception(ordemServico.ValidarRemocao);

            Assert.Null(excecao);
        }

        [Fact]
        public void ValidarRemocao_OsEmAndamento_DeveLancarExcecao()
        {
            var ordemServico = CriarComStatus(StatusOs.EmDiagnostico);

            Assert.Throws<InvalidOperationException>(ordemServico.ValidarRemocao);
        }

        [Theory]
        [InlineData(StatusOs.Recebida)]
        [InlineData(StatusOs.EmDiagnostico)]
        public void ValidarAlteracaoItens_RecebidaOuEmDiagnostico_NaoDeveLancarExcecao(StatusOs status)
        {
            var ordemServico = CriarComStatus(status);

            var excecao = Record.Exception(ordemServico.ValidarAlteracaoItens);

            Assert.Null(excecao);
        }

        [Theory]
        [InlineData(StatusOs.AguardandoAprovacao)]
        [InlineData(StatusOs.EmExecucao)]
        [InlineData(StatusOs.Finalizada)]
        [InlineData(StatusOs.Entregue)]
        public void ValidarAlteracaoItens_APartirDeAguardandoAprovacao_DeveLancarExcecao(StatusOs status)
        {
            var ordemServico = CriarComStatus(status);

            Assert.Throws<InvalidOperationException>(ordemServico.ValidarAlteracaoItens);
        }

        [Theory]
        [InlineData(StatusOs.Recebida)]
        [InlineData(StatusOs.EmDiagnostico)]
        public void AtualizarValor_RecebidaOuEmDiagnostico_DeveAlterar(StatusOs status)
        {
            var ordemServico = CriarComStatus(status);

            ordemServico.AtualizarValor(150m);

            Assert.Equal(150m, ordemServico.Valor);
        }

        [Fact]
        public void AtualizarValor_AposAguardandoAprovacao_DeveLancarExcecao()
        {
            var ordemServico = CriarComStatus(StatusOs.AguardandoAprovacao);

            Assert.Throws<InvalidOperationException>(() => ordemServico.AtualizarValor(150m));
            Assert.Equal(0, ordemServico.Valor);
        }
    }
}
