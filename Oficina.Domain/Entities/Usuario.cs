using Oficina.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oficina.Domain.Entities
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }
}
