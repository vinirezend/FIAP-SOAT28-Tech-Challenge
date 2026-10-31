using Dapper;
using Microsoft.Extensions.Configuration;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces.Repositories;
using Oficina.Infrastructure.Models;

namespace Oficina.Infrastructure.Repositories
{
    public class StatusRepository : BaseRepository, IStatusRepository
    {
        public StatusRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {

        }

        public async Task<Status?> ObterPorIdAsync(int idStatus, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT id_status, nome FROM status WHERE id_status = @IdStatus";

            var row = await QueryFirstOrDefaultAsync<StatusRow>(sql, new { IdStatus = idStatus }, cancellationToken);

            return row is null ? null : Status.Reconstituir(row.IdStatus, row.Nome);
        }

        public async Task<IReadOnlyList<Status>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT id_status, nome FROM status ORDER BY id_status";

            var rows = await QueryAsync<StatusRow>(sql, cancellationToken: cancellationToken);

            return rows.Select(row => Status.Reconstituir(row.IdStatus, row.Nome)).ToList();
        }
    }
}
