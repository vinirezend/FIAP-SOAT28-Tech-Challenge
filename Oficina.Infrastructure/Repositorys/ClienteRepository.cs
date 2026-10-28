using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.ValueObjects;
using Oficina.Infrastructure.Models;

namespace Oficina.Infrastructure.Repositorys;

public class ClienteRepository(IConfiguration configuration) : BaseRepository(configuration), IClienteRepository
{
    public async Task<Cliente> AdicionarAsync(Cliente cliente)
    {
        const string sql = "INSERT INTO cliente (nome, numero_documento, tipo_documento, email, telefone) VALUES (@Nome, @NumeroDocumento, @TipoDocumento, @Email, @Telefone) RETURNING id_cliente";

        try
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            var idCliente = await connection.ExecuteScalarAsync<int>(sql, new
            {
                cliente.Nome,
                NumeroDocumento = cliente.Documento.Numero,
                TipoDocumento = (int)cliente.Documento.Tipo,
                Email = cliente.Email.Endereco,
                Telefone = cliente.Telefone.Numero
            });
            return Cliente.Reconstituir(
                idCliente,
                cliente.Nome,
                cliente.Documento,
                cliente.Email,
                cliente.Telefone
            );

        }
        catch (PostgresException ex) when (ex.SqlState is "23505")
        {
            throw new InvalidOperationException("Ja existe um cliente com este documento.");
        }
    }

    public async Task AtualizarAsync(Cliente cliente)
    {
        const string sql = $"UPDATE cliente SET nome = @Nome, numero_documento = @NumeroDocumento, tipo_documento = @TipoDocumento, email = @Email, telefone = @Telefone WHERE id_cliente = @IdCliente";
        try
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                cliente.IdCliente,
                cliente.Nome,
                NumeroDocumento = cliente.Documento.Numero,
                TipoDocumento = (int)cliente.Documento.Tipo,
                Email = cliente.Email.Endereco,
                Telefone = cliente.Telefone.Numero
            });
        }
        catch (PostgresException ex) when (ex.SqlState is "23505")
        {
            throw new InvalidOperationException("Ja existe um cliente com este documento.");
        }
    }

    public async Task<Cliente?> ObterPorDocumentoAsync(string documento)
    {
        const string sql = $"SELECT * FROM cliente WHERE numero_documento = @documento";

        using var connection = await _dataSource.OpenConnectionAsync();
        var row = await connection.QueryFirstOrDefaultAsync<ClienteRow>(sql, new { documento });
        if (row is null)
            return null;
        return Cliente.Reconstituir(
            row.IdCliente,
            row.Nome,
            Documento.Criar(row.NumeroDocumento),
            Email.Criar(row.Email),
            Telefone.Criar(row.Telefone)
        );
    }

    public async Task<Cliente?> ObterPorIdAsync(int idCliente)
    {
        const string sql = $"SELECT * FROM cliente WHERE id_cliente = @idCliente";

        using var connection = await _dataSource.OpenConnectionAsync();
        var row = await connection.QueryFirstOrDefaultAsync<ClienteRow>(sql, new { idCliente });
        if (row is null)
            return null;
        return Cliente.Reconstituir(
            row.IdCliente,
            row.Nome,
            Documento.Criar(row.NumeroDocumento),
            Email.Criar(row.Email),
            Telefone.Criar(row.Telefone)
        );
    }

    public async Task<IReadOnlyList<Cliente>> ObterTodosAsync()
    {
        const string sql = @"
              SELECT 
                id_cliente IdCliente,
                nome Nome,
                numero_documento NumeroDocumento,
                tipo_documento TipoDocumento,
                email Email,
                telefone AS Telefone 
              FROM cliente;
            ";

        using var connection = await _dataSource.OpenConnectionAsync();
        var rows = await connection.QueryAsync<ClienteRow>(sql);
        return [.. rows.Select(row => Cliente.Reconstituir(
            row.IdCliente,
            row.Nome,
            Documento.Criar(row.NumeroDocumento),
            Email.Criar(row.Email),
            Telefone.Criar(row.Telefone)
        ))];
    }

    public async Task RemoverAsync(int idCliente)
    {

        const string sql = $"DELETE FROM cliente WHERE id_cliente = @idCliente";
        try
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            await connection.ExecuteAsync(sql, new { idCliente });
        }
        catch (PostgresException ex) when (ex.SqlState is "23503")
        {
            throw new InvalidOperationException("Nao foi possivel remover o cliente.");
        }
    }
}