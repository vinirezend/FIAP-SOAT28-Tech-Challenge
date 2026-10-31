using Npgsql;
using Oficina.Application.Exceptions;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces.Repositories;
using Oficina.Domain.ValueObjects;
using Oficina.Infrastructure.Models;

namespace Oficina.Infrastructure.Repositories
{
    public class ClienteRepository : BaseRepository, IClienteRepository
    {
        public ClienteRepository(IUnitOfWork unitOfWork) 
            : base(unitOfWork)
        {

        }

        public async Task<Cliente> AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO cliente (nome, numero_documento, tipo_documento, email, telefone) VALUES (@Nome, @NumeroDocumento, @TipoDocumento, @Email, @Telefone) RETURNING id_cliente";

            try
            {
                var idCliente = await ExecuteScalarAsync<int>(sql, new
                {
                    cliente.Nome,
                    NumeroDocumento = cliente.Documento.Numero,
                    TipoDocumento = (int)cliente.Documento.Tipo,
                    Email = cliente.Email.Endereco,
                    Telefone = cliente.Telefone.Numero
                }, cancellationToken);

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
                throw new BusinessException("Ja existe um cliente com este documento.");
            }
        }

        public async Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            const string sql = $"UPDATE cliente SET nome = @Nome, numero_documento = @NumeroDocumento, tipo_documento = @TipoDocumento, email = @Email, telefone = @Telefone WHERE id_cliente = @IdCliente";
            try
            {
                await ExecuteAsync(sql, new
                {
                    cliente.IdCliente,
                    cliente.Nome,
                    NumeroDocumento = cliente.Documento.Numero,
                    TipoDocumento = (int)cliente.Documento.Tipo,
                    Email = cliente.Email.Endereco,
                    Telefone = cliente.Telefone.Numero
                }, cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState is "23505")
            {
                throw new BusinessException("Ja existe um cliente com este documento.");
            }
        }

        public async Task<Cliente?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default)
        {
            const string sql = $"SELECT * FROM cliente WHERE numero_documento = @documento";

            var row = await QueryFirstOrDefaultAsync<ClienteRow>(sql, new { documento }, cancellationToken);

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

        public async Task<Cliente?> ObterPorIdAsync(int idCliente, CancellationToken cancellationToken = default)
        {
            const string sql = $"SELECT * FROM cliente WHERE id_cliente = @idCliente";

            var row = await QueryFirstOrDefaultAsync<ClienteRow>(sql, new { idCliente }, cancellationToken);

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

        public async Task<IReadOnlyList<Cliente>> ObterTodosAsync(CancellationToken cancellationToken = default)
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

            var rows = await QueryAsync<ClienteRow>(sql, cancellationToken: cancellationToken);
            return [.. rows.Select(row => Cliente.Reconstituir(
                row.IdCliente,
                row.Nome,
                Documento.Criar(row.NumeroDocumento),
                Email.Criar(row.Email),
                Telefone.Criar(row.Telefone)
            ))];
        }

        public async Task RemoverAsync(int idCliente, CancellationToken cancellationToken = default)
        {

            const string sql = $"DELETE FROM cliente WHERE id_cliente = @idCliente";
            try
            {
                await ExecuteAsync(sql, new { idCliente }, cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new BusinessException("Nao foi possivel remover o cliente.");
            }
        }
    }
}
