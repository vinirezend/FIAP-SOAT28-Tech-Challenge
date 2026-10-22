
namespace Oficina.Application.DTOs
{
    public class VeiculoDTO
    {
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Ano { get; set; }
        public string Placa { get; set; }
        public int IdVeiculo { get; set; }
        public int IdCliente { get; set; }
        public int IdTipoVeiculo { get; set; }
    }
}