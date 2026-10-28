using System;
using System.Collections.Generic;
using System.Text;

namespace Oficina.Application.Interfaces.Resources
{
    public interface ICurrentUser
    {
        int? IdUsuario { get; }
        int? IdColaborador { get; }
        int? IdDepartamento { get; }
        string? Cpf { get; }
        string? Email { get; }
        string? Login { get; }
        bool Autenticado { get; }
    }
}
