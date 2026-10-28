using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Infrastructure.Models;

namespace Oficina.Infrastructure.Repositorys
{
    public class OrdemServicoRepository : BaseRepository, IOrdemServicoRepository
    {
        // O status atual da OS é o último registro do seu histórico em os_status.
        private const string SelectOrdemServico = @"SELECT os.id_os, os.id_veiculo, os.data_inicio, os.data_fim, os.valor, ultimo_status.id_status
                                                    FROM os
                                                        LEFT JOIN LATERAL (
                                                            SELECT os_status.id_status
                                                            FROM os_status
                                                            WHERE os_status.id_os = os.id_os
                                                            ORDER BY os_status.id_os_status DESC
                                                            LIMIT 1
                                                        ) ultimo_status ON TRUE";

        private const string InsertOsStatus = "INSERT INTO os_status (id_os, id_status, id_colaborador, data_inclusao) VALUES (@IdOs, @IdStatus, @IdColaborador, @DataInclusao)";

        public OrdemServicoRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<OrdemServico> AdicionarAsync(OrdemServico ordemServico, int idColaborador, DateTime dataInclusao)
        {
            const string sql = "INSERT INTO os (id_veiculo, data_inicio, data_fim, valor) VALUES (@IdVeiculo, @DataInicio, @DataFim, @Valor) RETURNING id_os";
            try
            {
                using (var connection = await _dataSource.OpenConnectionAsync())
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    var idOs = await connection.ExecuteScalarAsync<int>(sql, new
                    {
                        ordemServico.IdVeiculo,
                        ordemServico.DataInicio,
                        ordemServico.DataFim,
                        ordemServico.Valor
                    }, transaction);

                    await connection.ExecuteAsync(InsertOsStatus, new
                    {
                        IdOs = idOs,
                        IdStatus = (int?)ordemServico.Status,
                        IdColaborador = idColaborador,
                        DataInclusao = dataInclusao
                    }, transaction);

                    await transaction.CommitAsync();

                    return OrdemServico.Reconstituir(idOs, ordemServico.IdVeiculo, ordemServico.DataInicio, ordemServico.DataFim, ordemServico.Valor, ordemServico.Status);
                }
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException(MensagemReferenciaInvalida(ex));
            }
        }

        public async Task AtualizarAsync(OrdemServico ordemServico)
        {
            const string sql = "UPDATE os SET id_veiculo = @IdVeiculo WHERE id_os = @IdOs";
            try
            {
                using (var connection = await _dataSource.OpenConnectionAsync())
                {
                    await connection.ExecuteAsync(sql, new
                    {
                        ordemServico.IdOs,
                        ordemServico.IdVeiculo
                    });
                }
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException(MensagemReferenciaInvalida(ex));
            }
        }

        public async Task AlterarStatusAsync(OrdemServico ordemServico, int idColaborador, DateTime dataInclusao)
        {
            const string sql = "UPDATE os SET data_inicio = @DataInicio, data_fim = @DataFim WHERE id_os = @IdOs";
            try
            {
                using (var connection = await _dataSource.OpenConnectionAsync())
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    await connection.ExecuteAsync(sql, new
                    {
                        ordemServico.IdOs,
                        ordemServico.DataInicio,
                        ordemServico.DataFim
                    }, transaction);

                    await connection.ExecuteAsync(InsertOsStatus, new
                    {
                        ordemServico.IdOs,
                        IdStatus = (int?)ordemServico.Status,
                        IdColaborador = idColaborador,
                        DataInclusao = dataInclusao
                    }, transaction);

                    await transaction.CommitAsync();
                }
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException(MensagemReferenciaInvalida(ex));
            }
        }

        public async Task<OrdemServico?> ObterPorIdAsync(int idOs)
        {
            const string sql = $"{SelectOrdemServico} WHERE os.id_os = @IdOs";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var row = await connection.QueryFirstOrDefaultAsync<OrdemServicoRow>(sql, new { IdOs = idOs });
                return row is null ? null : Reconstituir(row);
            }
        }

        public async Task<IReadOnlyList<OrdemServico>> ObterTodosAsync()
        {
            const string sql = $"{SelectOrdemServico} ORDER BY os.id_os DESC";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var rows = await connection.QueryAsync<OrdemServicoRow>(sql);
                return rows.Select(Reconstituir).ToList();
            }
        }

        public async Task<IReadOnlyList<OrdemServico>> ObterPorVeiculoAsync(int idVeiculo)
        {
            const string sql = $"{SelectOrdemServico} WHERE os.id_veiculo = @IdVeiculo ORDER BY os.id_os DESC";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var rows = await connection.QueryAsync<OrdemServicoRow>(sql, new { IdVeiculo = idVeiculo });
                return rows.Select(Reconstituir).ToList();
            }
        }

        public async Task<IReadOnlyList<OrdemServicoStatus>> ObterHistoricoStatusAsync(int idOs)
        {
            const string sql = "SELECT id_os_status, id_os, id_status, id_colaborador, data_inclusao FROM os_status WHERE id_os = @IdOs ORDER BY id_os_status";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var rows = await connection.QueryAsync<OrdemServicoStatusRow>(sql, new { IdOs = idOs });
                return rows.Select(row => OrdemServicoStatus.Reconstituir(
                    row.IdOsStatus,
                    row.IdOs,
                    (StatusOs)row.IdStatus,
                    row.IdColaborador,
                    row.DataInclusao
                )).ToList();
            }
        }

        public async Task RemoverAsync(int idOs)
        {
            string[] comandos =
            [
                "DELETE FROM os_status WHERE id_os = @IdOs",
                "DELETE FROM os_colaborador WHERE id_os = @IdOs",
                "DELETE FROM os WHERE id_os = @IdOs"
            ];
            try
            {
                using (var connection = await _dataSource.OpenConnectionAsync())
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    foreach (var sql in comandos)
                        await connection.ExecuteAsync(sql, new { IdOs = idOs }, transaction);

                    await transaction.CommitAsync();
                }
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException("Não foi possível remover a OS, pois ela possui serviços ou insumos vinculados.");
            }
        }

        private static string MensagemReferenciaInvalida(PostgresException ex) => ex.ConstraintName switch
        {
            "fk_os_veiculo" => "Veículo não encontrado.",
            "fk_os_status_colaborador" => "Colaborador não encontrado.",
            "fk_os_status_status" => "Status não encontrado.",
            _ => "Registro relacionado não encontrado."
        };

        private static OrdemServico Reconstituir(OrdemServicoRow row) => OrdemServico.Reconstituir(
            row.IdOs,
            row.IdVeiculo,
            row.DataInicio,
            row.DataFim,
            row.Valor,
            (StatusOs?)row.IdStatus
        );
    }
}
