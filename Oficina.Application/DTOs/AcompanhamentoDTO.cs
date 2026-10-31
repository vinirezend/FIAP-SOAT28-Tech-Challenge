namespace Oficina.Application.DTOs
{
    /// <summary>
    /// Andamento da OS como o cliente vê: status atual e etapas do fluxo.
    /// </summary>
    public class AcompanhamentoDTO
    {
        public int IdOs { get; set; }
        public int? IdStatus { get; set; }
        public string? Status { get; set; }
        public string? DescricaoStatus { get; set; }
        public string ClienteNome { get; set; }
        public string ClienteEmail { get; set; }
        public string Placa { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Ano { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public decimal ValorTotal { get; set; }
        public IReadOnlyList<AcompanhamentoEtapaDTO> Etapas { get; set; } = [];
    }

    public class AcompanhamentoEtapaDTO
    {
        public int IdStatus { get; set; }
        public string Nome { get; set; }
        public SituacaoEtapa Situacao { get; set; }

        /// <summary>Última vez que a OS entrou nesta etapa (UTC); null se não passou por ela.</summary>
        public DateTime? Data { get; set; }
    }

    public enum SituacaoEtapa
    {
        Pendente,
        Atual,
        Concluida,

        /// <summary>Etapa pulada no fluxo (ex.: Em execução quando o orçamento é recusado).</summary>
        NaoRealizada
    }
}
