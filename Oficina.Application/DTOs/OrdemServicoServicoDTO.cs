namespace Oficina.Application.DTOs
{
    public class OrdemServicoServicoDTO
    {
        public int IdOsServico { get; set; }
        public int IdOs { get; set; }
        public int IdServico { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorCobrado { get; set; }
        public decimal ValorTotal { get; set; }
    }
}