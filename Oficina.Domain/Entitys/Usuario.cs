using Oficina.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oficina.Domain.Entitys
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Login { get; set; }
        public string Senha { get; set; }
        public bool Ativo { get; set; }
    }
}
