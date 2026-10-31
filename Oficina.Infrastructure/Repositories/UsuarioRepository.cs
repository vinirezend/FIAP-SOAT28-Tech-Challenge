using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces.Repositories;
using Oficina.Domain.Models;

namespace Oficina.Infrastructure.Repositories;

public class UsuarioRepository(IUnitOfWork unitOfWork) : BaseRepository(unitOfWork), IUsuarioRepository
{
    public Task<Usuario?> ObterAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id_usuario, login, ativo FROM usuario WHERE id_usuario = @idUsuario";

        return QueryFirstOrDefaultAsync<Usuario>(sql, new { idUsuario }, cancellationToken);
    }

    public Task<UsuarioAutenticacao?> ObterPorLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT
                                usuario.id_usuario AS IdUsuario,
                                usuario.login AS Login,
                                usuario.senha AS Senha,
                                usuario.ativo AS UsuarioAtivo,
                                colaborador.id_colaborador AS IdColaborador,
                                colaborador.cpf AS Cpf,
                                colaborador.email AS Email,
                                colaborador.id_departamento AS IdDepartamento,
                                colaborador.ativo AS ColaboradorAtivo
                            FROM usuario
                                LEFT JOIN colaborador
                                    ON colaborador.id_usuario = usuario.id_usuario
                            WHERE usuario.login = @login;";

        return QueryFirstOrDefaultAsync<UsuarioAutenticacao>(sql, new { login }, cancellationToken);
    }
}
