using Oficina.Application.DTOs;
using System.Security.Claims;

namespace Oficina.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GerarTokenAcesso(IEnumerable<Claim> claims);

        Task<TokenDTO> ValidarCredenciais(UsuarioCredenciaisDTO usuarioCredenciaisDTO);
    }
}
