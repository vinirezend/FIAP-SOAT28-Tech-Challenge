using Oficina.Domain.Enum;

namespace Oficina.Domain.Entities
{
    public class OrdemServico
    {
        public int IdOs { get; private set; }
        public int IdVeiculo { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public decimal Valor { get; private set; }
        public StatusOs? Status { get; private set; }

        private OrdemServico() { }

        public static OrdemServico Criar(int idVeiculo)
        {
            return new OrdemServico
            {
                IdVeiculo = ValidarVeiculo(idVeiculo),
                Valor = 0,
                Status = StatusOs.Recebida
            };
        }

        public void AtualizarVeiculo(int idVeiculo)
        {
            if (Status is not (null or StatusOs.Recebida or StatusOs.EmDiagnostico))
                throw new InvalidOperationException("O veículo da OS só pode ser alterado até o diagnóstico.");

            IdVeiculo = ValidarVeiculo(idVeiculo);
        }

        public void AtualizarValor(decimal valor)
        {
            ValidarAlteracaoItens();
            Valor = valor;
        }

        /// <summary>
        /// Avança a OS para o próximo status do fluxo. Registra o início da execução e a finalização.
        /// </summary>
        public void AlterarStatus(StatusOs novoStatus, DateTime dataAlteracao)
        {
            if (!System.Enum.IsDefined(novoStatus))
                throw new ArgumentException("Status da OS inválido.");

            if (Status == StatusOs.Entregue)
                throw new InvalidOperationException("A OS já foi entregue e não pode mudar de status.");

            var proximoStatus = Status is null ? StatusOs.Recebida : Status.Value + 1;
            if (novoStatus != proximoStatus)
                throw new InvalidOperationException($"Transição de status inválida: de {Status} para {novoStatus}. O próximo status permitido é {proximoStatus}.");

            if (novoStatus == StatusOs.EmExecucao)
                DataInicio = dataAlteracao;

            if (novoStatus == StatusOs.Finalizada)
                DataFim = dataAlteracao;

            Status = novoStatus;
        }

        /// <summary>
        /// Cliente aprovou o orçamento: a OS segue para execução.
        /// </summary>
        public void AprovarOrcamento(DateTime dataAprovacao)
        {
            ValidarAguardandoAprovacao();
            AlterarStatus(StatusOs.EmExecucao, dataAprovacao);
        }

        /// <summary>
        /// Cliente recusou o orçamento: a OS é finalizada sem passar pela execução.
        /// </summary>
        public void RecusarOrcamento(DateTime dataRecusa)
        {
            ValidarAguardandoAprovacao();
            DataFim = dataRecusa;
            Status = StatusOs.Finalizada;
        }

        private void ValidarAguardandoAprovacao()
        {
            if (Status != StatusOs.AguardandoAprovacao)
                throw new InvalidOperationException("O orçamento desta OS não está aguardando aprovação.");
        }

        public void ValidarRemocao()
        {
            if (Status is not (null or StatusOs.Recebida))
                throw new InvalidOperationException("Somente OS com status Recebida pode ser removida.");
        }

        private static int ValidarVeiculo(int idVeiculo)
        {
            if (idVeiculo <= 0)
                throw new ArgumentException("Veículo da OS é obrigatório.");

            return idVeiculo;
        }

        public void ValidarAlteracaoItens()
        {
            if (Status is not (StatusOs.Recebida or StatusOs.EmDiagnostico))
                throw new InvalidOperationException("Itens só podem ser incluídos, removidos ou alterados enquanto a OS está em Recebida ou Em Diagnóstico.");
        }

        public static OrdemServico Reconstituir(int idOs, int idVeiculo, DateTime? dataInicio, DateTime? dataFim, decimal valor, StatusOs? status) =>
        new() { IdOs = idOs, IdVeiculo = idVeiculo, DataInicio = dataInicio, DataFim = dataFim, Valor = valor, Status = status };
    }
}
