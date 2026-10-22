using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oficina.Api.DTO.Veiculo
{
    public class VeiculoRequest
    {
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public string Marca { get; set; }
        public int Ano { get; set; }
        public int IdCliente { get; set; }
        public int IdTipoVeiculo { get; set; }
    }
}