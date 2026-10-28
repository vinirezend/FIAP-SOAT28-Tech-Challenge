using Dapper;
using Microsoft.Extensions.Configuration;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Infrastructure.Models;

namespace Oficina.Infrastructure.Repositorys
{
    public class StatusRepository : BaseRepository, IStatusRepository
    {
        public StatusRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<Status?> ObterPorIdAsync(int idStatus)
        {
            const string sql = "SELECT id_status, nome FROM status WHERE id_status = @IdStatus";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var row = await connection.QueryFirstOrDefaultAsync<StatusRow>(sql, new { IdStatus = idStatus });
                return row is null ? null : Status.Reconstituir(row.IdStatus, row.Nome);
            }
        }

        public async Task<IReadOnlyList<Status>> ObterTodosAsync()
        {
            const string sql = "SELECT id_status, nome FROM status ORDER BY id_status";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var rows = await connection.QueryAsync<StatusRow>(sql);
                return rows.Select(row => Status.Reconstituir(row.IdStatus, row.Nome)).ToList();
            }
        }
    }
}
