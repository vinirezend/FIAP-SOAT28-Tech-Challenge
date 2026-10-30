namespace Oficina.Api.DTO.Orcamento
{
    public class OrcamentoResponse
    {
        public int IdOs { get; set; }
        public int? IdStatus { get; set; }
        public string? Status { get; set; }
        public string ClienteNome { get; set; }
        public string ClienteEmail { get; set; }
        public string Placa { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Ano { get; set; }
        public IEnumerable<OrcamentoItemResponse> Servicos { get; set; } = [];
        public IEnumerable<OrcamentoItemResponse> Insumos { get; set; } = [];
        public decimal ValorTotal { get; set; }
    }

    public class OrcamentoItemResponse
    {
        public string Nome { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
