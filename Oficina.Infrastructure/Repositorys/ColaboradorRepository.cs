using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.ValueObjects;
using Oficina.Infrastructure.Models;

namespace Oficina.Infrastructure.Repositorys
{
    public class ColaboradorRepository : BaseRepository, IColaboradorRepository
    {
        private const string Colunas = "id_colaborador, nome, cpf, email, ativo, id_departamento, id_usuario";

        public ColaboradorRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<Colaborador> AdicionarAsync(Colaborador colaborador)
        {
            const string sql = "INSERT INTO colaborador (nome, cpf, email, ativo, id_departamento, id_usuario) VALUES (@Nome, @Cpf, @Email, @Ativo, @IdDepartamento, @IdUsuario) RETURNING id_colaborador";
            try
            {
                using (var connection = await _dataSource.OpenConnectionAsync())
                {
                    var idColaborador = await connection.ExecuteScalarAsync<int>(sql, new
                    {
                        colaborador.Nome,
                        Cpf = colaborador.Cpf.Numero,
                        Email = colaborador.Email.Endereco,
                        colaborador.Ativo,
                        IdDepartamento = (int)colaborador.IdDepartamento,
                        colaborador.IdUsuario
                    });

                    return Colaborador.Reconstituir(idColaborador, colaborador.Nome, colaborador.Cpf, colaborador.Email, colaborador.Ativo, colaborador.IdDepartamento, colaborador.IdUsuario);
                }
            }
            catch (PostgresException ex) when (ex.SqlState is "23505")
            {
                throw new InvalidOperationException(MensagemDuplicidade(ex));
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException("Usuário não encontrado.");
            }
        }

        public async Task AtualizarAsync(Colaborador colaborador)
        {
            const string sql = "UPDATE colaborador SET nome = @Nome, email = @Email, ativo = @Ativo, id_departamento = @IdDepartamento, id_usuario = @IdUsuario WHERE id_colaborador = @IdColaborador";
            try
            {
                using (var connection = await _dataSource.OpenConnectionAsync())
                {
                    await connection.ExecuteAsync(sql, new
                    {
                        colaborador.IdColaborador,
                        colaborador.Nome,
                        Email = colaborador.Email.Endereco,
                        colaborador.Ativo,
                        IdDepartamento = (int)colaborador.IdDepartamento,
                        colaborador.IdUsuario
                    });
                }
            }
            catch (PostgresException ex) when (ex.SqlState is "23505")
            {
                throw new InvalidOperationException(MensagemDuplicidade(ex));
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException("Usuário não encontrado.");
            }
        }

        public async Task<Colaborador?> ObterPorIdAsync(int idColaborador)
        {
            const string sql = $"SELECT {Colunas} FROM colaborador WHERE id_colaborador = @IdColaborador";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var row = await connection.QueryFirstOrDefaultAsync<ColaboradorRow>(sql, new { IdColaborador = idColaborador });
                return row is null ? null : Reconstituir(row);
            }
        }

        public async Task<IReadOnlyList<Colaborador>> ObterTodosAsync()
        {
            const string sql = $"SELECT {Colunas} FROM colaborador ORDER BY nome";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var rows = await connection.QueryAsync<ColaboradorRow>(sql);
                return rows.Select(Reconstituir).ToList();
            }
        }

        public async Task RemoverAsync(int idColaborador)
        {
            const string sql = "DELETE FROM colaborador WHERE id_colaborador = @IdColaborador";
            try
            {
                using (var connection = await _dataSource.OpenConnectionAsync())
                {
                    await connection.ExecuteAsync(sql, new { IdColaborador = idColaborador });
                }
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException("Não foi possível remover o colaborador, pois ele está vinculado a ordens de serviço. Desative-o.");
            }
        }

        private static string MensagemDuplicidade(PostgresException ex) => ex.ConstraintName switch
        {
            "uq_colaborador_cpf" => "Já existe um colaborador com esse CPF.",
            "uq_colaborador_email" => "Já existe um colaborador com esse e-mail.",
            _ => "Colaborador duplicado."
        };

        private static Colaborador Reconstituir(ColaboradorRow row) => Colaborador.Reconstituir(
            row.IdColaborador,
            row.Nome,
            Documento.Criar(row.Cpf),
            Email.Criar(row.Email),
            row.Ativo,
            (Departamento)row.IdDepartamento,
            row.IdUsuario
        );
    }
}
