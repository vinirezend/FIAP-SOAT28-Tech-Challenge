using Dapper;
using Microsoft.Extensions.Configuration;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Infrastructure.Repositorys
{
    public class UsuarioRepository : BaseRepository, IUsuarioRepository
    {
        public UsuarioRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<Usuario?> Obter(int idUsuario)
        {
            const string sql = "SELECT * FROM usuario WHERE idusuario = @idusuario";

            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { idUsuario });
            }
        }

        public async Task<Usuario?> ObterPorCredenciais(string login, string senha)
        {
            const string sql = "SELECT * FROM usuario WHERE login = @login AND senha = @senha";

            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { login, senha });
            }
        }
    }
}
