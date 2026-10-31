using Oficina.Domain.Entitys;
using Oficina.Domain.Models;

namespace Oficina.Domain.Interfaces.Repositorys;

public interface IUsuarioRepository
{
    /// <summary>
    /// Dados públicos do usuário (sem a senha).
    /// </summary>
    Task<Usuario?> ObterAsync(int idUsuario);

    /// <summary>
    /// Usuário com o hash da senha e o colaborador vinculado, para validar o login.
    /// </summary>
    Task<UsuarioAutenticacao?> ObterPorLoginAsync(string login);
}
