namespace Oficina.Api.DTO.Insumo
{
    public class InsumoCreateRequest
    {
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal ValorUnitario { get; set; }
        public int QuantidadeEstoque { get; set; }
    }
}
