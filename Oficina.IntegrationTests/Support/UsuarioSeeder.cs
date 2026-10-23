using Npgsql;
using Oficina.IntegrationTests.Hooks;

namespace Oficina.IntegrationTests.Support;

/// <summary>
/// Insere usuários diretamente no banco para preparar os cenários.
/// </summary>
public static class UsuarioSeeder
{
    public static async Task CriarAsync(string login, string senha, bool ativo = true)
    {
        const string sql = "INSERT INTO usuario (login, senha, ativo) VALUES (@login, @senha, @ativo)";

        await using var connection = new NpgsqlConnection(TestRunHooks.Database.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("login", login);
        command.Parameters.AddWithValue("senha", senha);
        command.Parameters.AddWithValue("ativo", ativo);

        await command.ExecuteNonQueryAsync();
    }
}
