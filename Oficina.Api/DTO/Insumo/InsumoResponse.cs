namespace Oficina.Api.DTO.Insumo
{
    public class InsumoResponse
    {
        public int IdInsumo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal ValorUnitario { get; set; }
        public bool Ativo { get; set; }
        public int QuantidadeEstoque { get; set; }
        public int QuantidadeReservada { get; set; }
    }
}
