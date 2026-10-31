using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Oficina.IntegrationTests.Infrastructure;

/// <summary>
/// Container PostgreSQL compartilhado por toda a execução dos testes.
/// O schema é criado a partir dos mesmos scripts usados pelo docker-compose (database/init).
/// </summary>
public sealed class PostgresDatabase : IAsyncDisposable
{
    private const string Image = "postgres:17-alpine";
    private const string InitScriptsFolder = "database/init";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image)
        .WithDatabase("oficina")
        .WithUsername("oficina")
        .WithPassword("oficina")
        .Build();

    private Respawner? _respawner;

    public string ConnectionString => _container.GetConnectionString();

    public async Task StartAsync()
    {
        await _container.StartAsync();
        await ExecutarScriptsInicializacaoAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            // Tabela de domínio (espelha o enum StatusOs): os históricos de status dependem dela
            TablesToIgnore = ["status"]
        });
    }

    /// <summary>
    /// Apaga os dados de todas as tabelas (mantendo o schema) para isolar os cenários.
    /// </summary>
    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner!.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    private async Task ExecutarScriptsInicializacaoAsync()
    {
        var pasta = Path.Combine(AppContext.BaseDirectory, InitScriptsFolder);
        var scripts = Directory.GetFiles(pasta, "*.sql").OrderBy(arquivo => arquivo, StringComparer.Ordinal);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        foreach (var script in scripts)
        {
            await using var command = new NpgsqlCommand(await File.ReadAllTextAsync(script), connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
