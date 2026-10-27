using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Oficina.Application.Interfaces.Resources;

namespace Oficina.Api.Filters;

/// <summary>
/// Libera o acesso à action apenas com um link pré-assinado válido (gerado pelo ILinkAssinadoGenerator).
/// Use junto com [AllowAnonymous] para dispensar o token JWT.
/// </summary>
public class LinkAssinadoAttribute : TypeFilterAttribute
{
    public LinkAssinadoAttribute() : base(typeof(LinkAssinadoFilter))
    {
    }
}

public class LinkAssinadoFilter(ILinkAssinadoGenerator linkAssinadoGenerator) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;

        var status = linkAssinadoGenerator.Validar(
            request.Path.Value ?? string.Empty,
            request.Query[ParametrosLinkAssinado.Expira],
            request.Query[ParametrosLinkAssinado.Assinatura]);

        context.Result = status switch
        {
            StatusLinkAssinado.Expirado => new ObjectResult(new { mensagem = "Este link expirou. Solicite um novo link à oficina." })
            {
                StatusCode = StatusCodes.Status410Gone
            },
            StatusLinkAssinado.Invalido => new ObjectResult(new { mensagem = "Link inválido." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            },
            _ => null
        };
    }
}
