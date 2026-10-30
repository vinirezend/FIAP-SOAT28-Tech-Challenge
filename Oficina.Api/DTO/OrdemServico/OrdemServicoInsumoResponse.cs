namespace Oficina.Api.DTO.OrdemServico
{
    public class OrdemServicoInsumoResponse
    {
        public int IdOsInsumo { get; set; }
        public int IdOs { get; set; }
        public int IdInsumo { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorCobrado { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
