using Dapper;
using Microsoft.Extensions.Configuration;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.Models;

namespace Oficina.Infrastructure.Repositorys
{
    public class UsuarioRepository : BaseRepository, IUsuarioRepository
    {
        public UsuarioRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<Usuario?> Obter(int idUsuario)
        {
            const string sql = "SELECT * FROM usuario WHERE id_usuario = @idusuario";

            return await QueryFirstOrDefaultAsync<Usuario>(sql, new { idUsuario });
        }

        public async Task<UsuarioAutenticacao?> ObterPorCredenciais(string login, string senha)
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
                                WHERE usuario.login = @login AND usuario.senha = @senha;";

            return await QueryFirstOrDefaultAsync<UsuarioAutenticacao>(sql, new { login, senha });
        }
    }
}
