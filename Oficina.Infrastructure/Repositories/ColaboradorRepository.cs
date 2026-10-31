using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entities;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositories;
using Oficina.Domain.ValueObjects;
using Oficina.Infrastructure.Models;

namespace Oficina.Infrastructure.Repositories
{
    public class ColaboradorRepository : BaseRepository, IColaboradorRepository
    {
        private const string Colunas = "id_colaborador, nome, cpf, email, ativo, id_departamento, id_usuario";

        public ColaboradorRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {

        }

        public async Task<Colaborador> AdicionarAsync(Colaborador colaborador, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO colaborador (nome, cpf, email, ativo, id_departamento, id_usuario) VALUES (@Nome, @Cpf, @Email, @Ativo, @IdDepartamento, @IdUsuario) RETURNING id_colaborador";
            try
            {
                var idColaborador = await ExecuteScalarAsync<int>(sql, new
                {
                    colaborador.Nome,
                    Cpf = colaborador.Cpf.Numero,
                    Email = colaborador.Email.Endereco,
                    colaborador.Ativo,
                    IdDepartamento = (int)colaborador.IdDepartamento,
                    colaborador.IdUsuario
                }, cancellationToken);

                return Colaborador.Reconstituir(idColaborador, colaborador.Nome, colaborador.Cpf, colaborador.Email, colaborador.Ativo, colaborador.IdDepartamento, colaborador.IdUsuario);
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

        public async Task AtualizarAsync(Colaborador colaborador, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE colaborador SET nome = @Nome, email = @Email, ativo = @Ativo, id_departamento = @IdDepartamento, id_usuario = @IdUsuario WHERE id_colaborador = @IdColaborador";
            try
            {
                await ExecuteAsync(sql, new
                {
                    colaborador.IdColaborador,
                    colaborador.Nome,
                    Email = colaborador.Email.Endereco,
                    colaborador.Ativo,
                    IdDepartamento = (int)colaborador.IdDepartamento,
                    colaborador.IdUsuario
                }, cancellationToken);
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

        public async Task<Colaborador?> ObterPorIdAsync(int idColaborador, CancellationToken cancellationToken = default)
        {
            const string sql = $"SELECT {Colunas} FROM colaborador WHERE id_colaborador = @IdColaborador";

            var row = await QueryFirstOrDefaultAsync<ColaboradorRow>(sql, new { IdColaborador = idColaborador }, cancellationToken);

            return row is null ? null : Reconstituir(row);
        }

        public async Task<IReadOnlyList<Colaborador>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            const string sql = $"SELECT {Colunas} FROM colaborador ORDER BY nome";

            var rows = await QueryAsync<ColaboradorRow>(sql, cancellationToken: cancellationToken);

            return rows.Select(Reconstituir).ToList();
        }

        public async Task RemoverAsync(int idColaborador, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM colaborador WHERE id_colaborador = @IdColaborador";
            try
            {
                await ExecuteAsync(sql, new { IdColaborador = idColaborador }, cancellationToken);
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
