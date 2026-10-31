using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Oficina.Api.ErrorHandling;
using Oficina.Application.Interfaces.Resources;

namespace Oficina.Api.Filters;

/// <summary>
/// Libera o acesso à action apenas com um link pré-assinado válido (gerado pelo ILinkAssinadoGenerator).
/// Use junto com [AllowAnonymous] para dispensar o token JWT.
/// </summary>
public class LinkAssinadoAttribute() : TypeFilterAttribute(typeof(LinkAssinadoFilter));

/// <summary>
/// Quem abre o link é o cliente no navegador, por isso a recusa é uma página HTML e não JSON.
/// </summary>
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
            StatusLinkAssinado.Expirado => PaginaHtml.Mensagem("Link expirado", "Este link expirou. Solicite um novo link à oficina.", StatusCodes.Status410Gone),
            StatusLinkAssinado.Invalido => PaginaHtml.Mensagem("Link inválido", "Confira se o endereço foi copiado por completo ou solicite um novo link à oficina.", StatusCodes.Status403Forbidden),
            _ => null
        };
    }
}
