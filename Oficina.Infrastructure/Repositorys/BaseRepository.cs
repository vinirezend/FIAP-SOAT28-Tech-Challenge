using Npgsql;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace Oficina.Infrastructure.Repositorys
{
    public class BaseRepository
    {
        protected readonly NpgsqlDataSource _dataSource;

        static BaseRepository()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public BaseRepository(IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            _dataSource = NpgsqlDataSource.Create(connectionString);
        }
    }
}
