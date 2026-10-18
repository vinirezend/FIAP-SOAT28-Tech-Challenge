using Npgsql;
using Microsoft.Extensions.Configuration;

namespace Oficina.Infrastructure.Repositorys
{
    public class BaseRepository
    {
        protected readonly NpgsqlDataSource _dataSource;

        public BaseRepository(IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            _dataSource = NpgsqlDataSource.Create(connectionString);
        }
    }
}
