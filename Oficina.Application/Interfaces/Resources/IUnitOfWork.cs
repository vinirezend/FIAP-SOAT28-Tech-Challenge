using System.Data;
using System.Data.Common;

namespace Oficina.Application.Interfaces.Resources
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task<DbConnection> GetConnectionAsync();

        DbTransaction? Transaction { get; }

        Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default);
    }
}
