using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.DTO.Usuario;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Api.Controllers;

/// <summary>
/// Consulta de usuários de acesso ao sistema.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsuarioController(IUsuarioRepository usuarioRepository) : ControllerBase
{
    /// <summary>
    /// Obtém um usuário pelo id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<Results<Ok<UsuarioResponse>, NotFound>> ObterAsync(int id, CancellationToken cancellationToken) =>
        await usuarioRepository.ObterAsync(id, cancellationToken) is { } usuario
            ? TypedResults.Ok(new UsuarioResponse { IdUsuario = usuario.IdUsuario, Login = usuario.Login, Ativo = usuario.Ativo })
            : TypedResults.NotFound();
}
