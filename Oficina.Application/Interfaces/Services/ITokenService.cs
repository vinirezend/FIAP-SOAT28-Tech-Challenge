using Oficina.Application.DTOs;
using System.Security.Claims;

namespace Oficina.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GerarTokenAcesso(IEnumerable<Claim> claims);

        TokenDTO ValidarCredenciais(UsuarioCredenciaisDTO usuarioCredenciaisDTO);
    }
}
