namespace Oficina.Application.DTOs
{
    public class OrdemServicoDTO
    {
        public int IdOs { get; set; }
        public int IdVeiculo { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public decimal Valor { get; set; }
        public int? IdStatus { get; set; }
        public string? Status { get; set; }
    }
}
