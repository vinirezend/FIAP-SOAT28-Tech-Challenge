using Dapper;
using Microsoft.Extensions.Configuration;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Infrastructure.Models;

namespace Oficina.Infrastructure.Repositorys
{
    public class StatusRepository : BaseRepository, IStatusRepository
    {
        public StatusRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {

        }

        public async Task<Status?> ObterPorIdAsync(int idStatus)
        {
            const string sql = "SELECT id_status, nome FROM status WHERE id_status = @IdStatus";

            var row = await QueryFirstOrDefaultAsync<StatusRow>(sql, new { IdStatus = idStatus });

            return row is null ? null : Status.Reconstituir(row.IdStatus, row.Nome);
        }

        public async Task<IReadOnlyList<Status>> ObterTodosAsync()
        {
            const string sql = "SELECT id_status, nome FROM status ORDER BY id_status";

            var rows = await QueryAsync<StatusRow>(sql);

            return rows.Select(row => Status.Reconstituir(row.IdStatus, row.Nome)).ToList();
        }
    }
}
