using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oficina.Api.DTO.Servico
{
    public class ServicoUpdateRequest
    {
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal ValorEstipulado { get; set; } 
        public bool Ativo { get; set; }
    }
}