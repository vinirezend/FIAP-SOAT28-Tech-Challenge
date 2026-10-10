using Npgsql;

namespace Oficina.Api.Extensions;

public static class DatabaseExtensions
{
    private const string ConnectionStringName = "DefaultConnection";

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' não configurada.");

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));

        return services;
    }
}
