using Npgsql;
using Oficina.Domain.Enum;
using Oficina.IntegrationTests.Hooks;

namespace Oficina.IntegrationTests.Support;

/// <summary>
/// Insere diretamente no banco os cadastros que os fluxos de OS pressupõem (colaborador, cliente, veículo, serviço e insumo).
/// </summary>
public static class OficinaSeeder
{
    public static async Task<int> CriarColaboradorAsync(string login, string senha, Departamento departamento)
    {
        var idUsuario = await UsuarioSeeder.CriarAsync(login, senha);

        return await InserirAsync(
            "INSERT INTO colaborador (nome, email, ativo, id_departamento, id_usuario) VALUES (@nome, @email, TRUE, @departamento, @idUsuario) RETURNING id_colaborador",
            ("nome", login),
            ("email", login),
            ("departamento", (int)departamento),
            ("idUsuario", idUsuario));
    }

    public static Task<int> CriarClienteAsync(string nome, string cpf, string email) => InserirAsync(
        "INSERT INTO cliente (nome, numero_documento, tipo_documento, email, telefone) VALUES (@nome, @cpf, @tipo, @email, '(11) 99999-9999') RETURNING id_cliente",
        ("nome", nome),
        ("cpf", cpf),
        ("tipo", (int)TipoDocumento.CPF),
        ("email", email));

    public static Task<int> CriarVeiculoAsync(int idCliente, string placa, string marca, string modelo) => InserirAsync(
        "INSERT INTO veiculo (id_cliente, placa, marca, modelo, ano, id_tipo_veiculo) VALUES (@idCliente, @placa, @marca, @modelo, 2020, @tipo) RETURNING id_veiculo",
        ("idCliente", idCliente),
        ("placa", placa),
        ("marca", marca),
        ("modelo", modelo),
        ("tipo", (int)TipoVeiculo.Carro));

    public static Task<int> CriarServicoAsync(string nome, decimal valor) => InserirAsync(
        "INSERT INTO servico (nome, descricao, ativo, valor_estipulado) VALUES (@nome, @nome, TRUE, @valor) RETURNING id_servico",
        ("nome", nome),
        ("valor", valor));

    public static Task<int> CriarInsumoAsync(string nome, decimal valor, int estoque) => InserirAsync(
        "INSERT INTO insumo (nome, descricao, ativo, quantidade_estoque, quantidade_reservada, valor_unitario) VALUES (@nome, @nome, TRUE, @estoque, 0, @valor) RETURNING id_insumo",
        ("nome", nome),
        ("valor", valor),
        ("estoque", estoque));

    public static async Task<int> ObterQuantidadeReservadaAsync(int idInsumo) =>
        (int)(await ExecutarEscalarAsync("SELECT quantidade_reservada FROM insumo WHERE id_insumo = @id", ("id", idInsumo)))!;

    private static async Task<int> InserirAsync(string sql, params (string Nome, object Valor)[] parametros) =>
        (int)(await ExecutarEscalarAsync(sql, parametros))!;

    private static async Task<object?> ExecutarEscalarAsync(string sql, params (string Nome, object Valor)[] parametros)
    {
        await using var connection = new NpgsqlConnection(TestRunHooks.Database.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var (nome, valor) in parametros)
            command.Parameters.AddWithValue(nome, valor);

        return await command.ExecuteScalarAsync();
    }
}
