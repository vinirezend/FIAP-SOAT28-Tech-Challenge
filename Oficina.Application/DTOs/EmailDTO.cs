using System;
using System.Collections.Generic;
using System.Text;

namespace Oficina.Application.DTOs
{
    public class EmailDTO
    {
        public string ServidorSMTP { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public string EnderecoRemetente { get; set; }
        public string NomeRemetente { get; set; }
        public string Assunto { get; set; }
        public string Mensagem { get; set; }
        public List<string> EnderecoDestinatario { get; set; }
        public List<string> EnderecoCopia { get; set; }
        public int PortaServidorSMTP { get; set; }
        public bool SSL { get; set; }
    }
}
