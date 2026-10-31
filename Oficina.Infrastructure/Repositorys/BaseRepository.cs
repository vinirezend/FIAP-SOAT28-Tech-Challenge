using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Oficina.Application.Interfaces.Resources;

namespace Oficina.Infrastructure.Repositorys
{
    public class BaseRepository
    {
        protected readonly IUnitOfWork _unitOfWork;

        static BaseRepository()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        protected BaseRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
        {
            var connection = await _unitOfWork.GetConnectionAsync(cancellationToken);

            var command = new CommandDefinition(
                sql,
                parameters,
                transaction: _unitOfWork.Transaction,
                cancellationToken: cancellationToken);

            return await connection.QueryFirstOrDefaultAsync<T>(command);
        }

        protected async Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default)
        {
            var connection = await _unitOfWork.GetConnectionAsync(cancellationToken);

            var command = new CommandDefinition(
                sql,
                parameters,
                transaction: _unitOfWork.Transaction,
                cancellationToken: cancellationToken);

            return await connection.ExecuteAsync(command);
        }

        protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
        {
            var connection = await _unitOfWork.GetConnectionAsync(cancellationToken);

            var command = new CommandDefinition(
                sql,
                parameters,
                transaction: _unitOfWork.Transaction,
                cancellationToken: cancellationToken);

            return await connection.QueryAsync<T>(command);
        }

        protected async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
        {
            var connection = await _unitOfWork.GetConnectionAsync(cancellationToken);

            var command = new CommandDefinition(
                sql,
                parameters,
                transaction: _unitOfWork.Transaction,
                cancellationToken: cancellationToken);

            return await connection.ExecuteScalarAsync<T>(command);
        }
    }
}
