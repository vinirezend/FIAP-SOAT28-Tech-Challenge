using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Oficina.Api.Extensions;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;

namespace Oficina.Api.Controllers;

/// <summary>
/// Autenticação de usuários da oficina.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LoginController(ITokenGenerator tokenService) : ControllerBase
{
    /// <summary>
    /// Valida login e senha e retorna o token JWT para usar no header Authorization (Bearer).
    /// </summary>
    /// <remarks>
    /// Limitado a poucas tentativas por minuto por IP (429 ao exceder).
    /// </remarks>
    // ActionResult (e não TypedResults) para manter o token como text/plain, como os clientes já consomem
    [HttpPost("signin")]
    [EnableRateLimiting(RateLimitingExtensions.PoliticaLogin)]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/plain")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests, "application/problem+json")]
    public async Task<ActionResult<string>> SignIn(UsuarioCredenciaisDTO usuarioCredenciais) =>
        await tokenService.ValidarCredenciais(usuarioCredenciais) is { } token
            ? Ok(token.Token)
            : Problem("Login ou senha incorretos.", statusCode: StatusCodes.Status401Unauthorized);
}
