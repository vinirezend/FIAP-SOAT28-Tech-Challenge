using Npgsql;
using Oficina.Application.Interfaces.Resources;
using Oficina.Infrastructure.Repositorys;
using System.Data;
using System.Data.Common;

namespace Oficina.Infrastructure.Resources
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly NpgsqlDataSource _dataSource;

        private DbConnection? _connection;
        private DbTransaction? _transaction;

        public DbTransaction? Transaction => _transaction;

        public UnitOfWork(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_connection is null)
            {
                _connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            }

            return _connection;
        }

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
            {
                throw new InvalidOperationException(
                    "Já existe uma transação ativa nesta unidade de trabalho.");
            }

            var connection = await GetConnectionAsync(cancellationToken);

            await using var transaction =
                await connection.BeginTransactionAsync(cancellationToken);

            _transaction = transaction;

            try
            {
                var result = await operation(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                try
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                }
                catch
                {
                    // Não substituir a exceção original por uma falha no rollback.
                    // A conexão pode precisar ser descartada se seu estado for incerto.
                }

                throw;
            }
            finally
            {
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            _transaction = null;

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }
}
