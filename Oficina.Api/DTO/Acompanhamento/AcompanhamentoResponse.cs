namespace Oficina.Api.DTO.Acompanhamento
{
    public class AcompanhamentoResponse
    {
        public int IdOs { get; set; }
        public int? IdStatus { get; set; }
        public string? Status { get; set; }
        public string? DescricaoStatus { get; set; }
        public string ClienteNome { get; set; }
        public string Placa { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Ano { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public decimal ValorTotal { get; set; }
        public IEnumerable<AcompanhamentoEtapaResponse> Etapas { get; set; } = [];
    }

    public class AcompanhamentoEtapaResponse
    {
        public int IdStatus { get; set; }
        public string Nome { get; set; }

        /// <summary>Pendente, Atual, Concluida ou NaoRealizada.</summary>
        public string Situacao { get; set; }
        public DateTime? Data { get; set; }
    }
}
