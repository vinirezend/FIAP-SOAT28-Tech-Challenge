namespace Oficina.Application.DTOs
{
    public class OrcamentoDTO
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
        public IReadOnlyList<OrcamentoItemDTO> Servicos { get; set; } = [];
        public IReadOnlyList<OrcamentoItemDTO> Insumos { get; set; } = [];
        public decimal ValorTotal { get; set; }
    }

    public class OrcamentoItemDTO
    {
        public string Nome { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
