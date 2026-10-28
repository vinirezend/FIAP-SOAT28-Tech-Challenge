using Microsoft.AspNetCore.Http;
using Oficina.Application.Interfaces.Resources;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Oficina.Infrastructure.Resources
{
    public class CurrentUserService : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUserService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private ClaimsPrincipal? Usuario => _accessor.HttpContext?.User;

        public bool Autenticado => Usuario?.Identity?.IsAuthenticated == true;

        public int? IdUsuario => ObterInteiro(ClaimTypes.NameIdentifier);

        public int? IdColaborador => ObterInteiro("id_colaborador");

        public int? IdDepartamento =>ObterInteiro("id_departamento");

        public string? Cpf => Usuario?.FindFirst("cpf")?.Value;

        public string? Email => Usuario?.FindFirst(ClaimTypes.Email)?.Value;

        public string? Login => Usuario?.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;

        private int? ObterInteiro(string tipoClaim)
        {
            var valor = Usuario?.FindFirst(tipoClaim)?.Value;

            return int.TryParse(valor, out var resultado)
                ? resultado
                : null;
        }
    }
}
