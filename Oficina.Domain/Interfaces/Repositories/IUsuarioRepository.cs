using Oficina.Domain.Entities;
using Oficina.Domain.Models;

namespace Oficina.Domain.Interfaces.Repositories;

public interface IUsuarioRepository
{
    /// <summary>
    /// Dados públicos do usuário (sem a senha).
    /// </summary>
    Task<Usuario?> ObterAsync(int idUsuario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Usuário com o hash da senha e o colaborador vinculado, para validar o login.
    /// </summary>
    Task<UsuarioAutenticacao?> ObterPorLoginAsync(string login, CancellationToken cancellationToken = default);
}
