using Oficina.Application.DTOs;
using System.Security.Claims;

namespace Oficina.Application.Interfaces.Resources
{
    public interface ITokenGenerator
    {
        string GerarTokenAcesso(IEnumerable<Claim> claims);

        Task<TokenDTO?> ValidarCredenciais(UsuarioCredenciaisDTO usuarioCredenciaisDTO);
    }
}
