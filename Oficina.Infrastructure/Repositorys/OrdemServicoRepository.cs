using Npgsql;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.Models;
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

        // Soma os itens de os_servico e os_insumo direto no banco, dentro da mesma transação da alteração do item.
        private const string RecalcularValorOs = @"UPDATE os
                                                    SET valor = COALESCE((SELECT SUM(quantidade * valor_cobrado) FROM os_servico WHERE id_os = @IdOs), 0)
                                                              + COALESCE((SELECT SUM(quantidade * valor_cobrado) FROM os_insumo WHERE id_os = @IdOs), 0)
                                                    WHERE id_os = @IdOs";

        public OrdemServicoRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {

        }

        public async Task<OrdemServico> AdicionarAsync(OrdemServico ordemServico, int idColaborador, DateTime dataInclusao, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO os (id_veiculo, data_inicio, data_fim, valor) VALUES (@IdVeiculo, @DataInicio, @DataFim, @Valor) RETURNING id_os";
            try
            {
                int idOs = default;
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    idOs = await ExecuteScalarAsync<int>(sql, new
                    {
                        ordemServico.IdVeiculo,
                        ordemServico.DataInicio,
                        ordemServico.DataFim,
                        ordemServico.Valor
                    }, ct);

                    await ExecuteAsync(InsertOsStatus, new
                    {
                        IdOs = idOs,
                        IdStatus = (int?)ordemServico.Status,
                        IdColaborador = idColaborador,
                        DataInclusao = dataInclusao
                    }, ct);

                    return true;
                }, cancellationToken);

                return OrdemServico.Reconstituir(idOs, ordemServico.IdVeiculo, ordemServico.DataInicio, ordemServico.DataFim, ordemServico.Valor, ordemServico.Status);
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException(MensagemReferenciaInvalida(ex));
            }
        }

        public async Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE os SET id_veiculo = @IdVeiculo WHERE id_os = @IdOs";

            try
            {
                await ExecuteAsync(sql, new
                {
                    ordemServico.IdOs,
                    ordemServico.IdVeiculo
                }, cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException(MensagemReferenciaInvalida(ex));
            }
        }

        public async Task AlterarStatusAsync(OrdemServico ordemServico, int idColaborador, DateTime dataInclusao, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE os SET data_inicio = @DataInicio, data_fim = @DataFim WHERE id_os = @IdOs";
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    await ExecuteAsync(sql, new
                    {
                        ordemServico.IdOs,
                        ordemServico.DataInicio,
                        ordemServico.DataFim
                    }, ct);

                    await ExecuteAsync(InsertOsStatus, new
                    {
                        ordemServico.IdOs,
                        IdStatus = (int?)ordemServico.Status,
                        IdColaborador = idColaborador,
                        DataInclusao = dataInclusao
                    }, ct);

                    return true;
                }, cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException(MensagemReferenciaInvalida(ex));
            }
        }

        public async Task<OrdemServico?> ObterPorIdAsync(int idOs, CancellationToken cancellationToken = default)
        {
            const string sql = $"{SelectOrdemServico} WHERE os.id_os = @IdOs";

            var row = await QueryFirstOrDefaultAsync<OrdemServicoRow>(sql, new { IdOs = idOs }, cancellationToken);

            return row is null ? null : Reconstituir(row);
        }

        public async Task<IReadOnlyList<OrdemServico>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            const string sql = $"{SelectOrdemServico} ORDER BY os.id_os DESC";

            var rows = await QueryAsync<OrdemServicoRow>(sql, cancellationToken: cancellationToken);

            return rows.Select(Reconstituir).ToList();
        }

        public async Task<IReadOnlyList<OrdemServico>> ObterPorVeiculoAsync(int idVeiculo, CancellationToken cancellationToken = default)
        {
            const string sql = $"{SelectOrdemServico} WHERE os.id_veiculo = @IdVeiculo ORDER BY os.id_os DESC";

            var rows = await QueryAsync<OrdemServicoRow>(sql, new { IdVeiculo = idVeiculo }, cancellationToken);

            return rows.Select(Reconstituir).ToList();
        }

        public async Task<IReadOnlyList<OrdemServicoStatus>> ObterHistoricoStatusAsync(int idOs, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT id_os_status, id_os, id_status, id_colaborador, data_inclusao FROM os_status WHERE id_os = @IdOs ORDER BY id_os_status";

            var rows = await QueryAsync<OrdemServicoStatusRow>(sql, new { IdOs = idOs }, cancellationToken);

            return rows.Select(row => OrdemServicoStatus.Reconstituir(
                row.IdOsStatus,
                row.IdOs,
                (StatusOs)row.IdStatus,
                row.IdColaborador,
                row.DataInclusao
            )).ToList();
        }

        public async Task<TempoMedioExecucao> ObterTempoMedioExecucaoAsync(DateTime? finalizadaAPartirDe, DateTime? finalizadaAntesDe, int? idServico, CancellationToken cancellationToken = default)
        {
            // Só entram OS que passaram pela execução e foram finalizadas: as de orçamento recusado não têm data_inicio.
            const string sql = @"SELECT COUNT(*)::int AS quantidade_ordens,
                                        AVG(EXTRACT(EPOCH FROM os.data_fim - os.data_inicio))::float8 AS tempo_medio_segundos
                                 FROM os
                                 WHERE os.data_inicio IS NOT NULL
                                   AND os.data_fim IS NOT NULL
                                   AND (@FinalizadaAPartirDe::timestamp IS NULL OR os.data_fim >= @FinalizadaAPartirDe)
                                   AND (@FinalizadaAntesDe::timestamp IS NULL OR os.data_fim < @FinalizadaAntesDe)
                                   AND (@IdServico::int IS NULL OR EXISTS (
                                        SELECT 1 FROM os_servico
                                        WHERE os_servico.id_os = os.id_os AND os_servico.id_servico = @IdServico))";

            var row = await QueryFirstOrDefaultAsync<TempoMedioExecucaoRow>(sql, new
            {
                FinalizadaAPartirDe = finalizadaAPartirDe,
                FinalizadaAntesDe = finalizadaAntesDe,
                IdServico = idServico
            }, cancellationToken);

            return row?.TempoMedioSegundos is { } segundos
                ? new TempoMedioExecucao(row.QuantidadeOrdens, TimeSpan.FromSeconds(segundos))
                : new TempoMedioExecucao(0, null);
        }

        public async Task RemoverAsync(int idOs, CancellationToken cancellationToken = default)
        {
            string[] comandos =
            [
                "DELETE FROM os_servico WHERE id_os = @IdOs",
                "DELETE FROM os_insumo WHERE id_os = @IdOs",
                "DELETE FROM os_status WHERE id_os = @IdOs",
                "DELETE FROM os_colaborador WHERE id_os = @IdOs",
                "DELETE FROM os WHERE id_os = @IdOs"
            ];
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    foreach (var sql in comandos)
                        await ExecuteAsync(sql, new { IdOs = idOs }, ct);

                    return true;
                }, cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException("Não foi possível remover a OS, pois ela possui serviços ou insumos vinculados.");
            }
        }

        public async Task<IReadOnlyList<OrdemServicoServico>> ObterServicosAsync(int idOs, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT id_os_servico, id_os, id_servico, quantidade, valor_cobrado FROM os_servico WHERE id_os = @IdOs";

            var rows = await QueryAsync<OsServicoRow>(sql, new { IdOs = idOs }, cancellationToken);

            return rows.Select(Reconstituir).ToList();
        }

        public async Task<OrdemServicoServico> AdicionarServicoAsync(OrdemServicoServico servico, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO os_servico (id_os, id_servico, quantidade, valor_cobrado) VALUES (@IdOs, @IdServico, @Quantidade, @ValorCobrado) RETURNING id_os_servico";
            try
            {
                int idOsServico = default;
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    idOsServico = await ExecuteScalarAsync<int>(sql, new
                    {
                        servico.IdOs,
                        servico.IdServico,
                        servico.Quantidade,
                        servico.ValorCobrado
                    }, ct);

                    await ExecuteAsync(RecalcularValorOs, new { servico.IdOs }, ct);

                    return true;
                }, cancellationToken);

                return OrdemServicoServico.Reconstituir(idOsServico, servico.IdOs, servico.IdServico, servico.Quantidade, servico.ValorCobrado);
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException(MensagemReferenciaInvalida(ex));
            }
        }

        public async Task RemoverServicoAsync(OrdemServicoServico servico, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM os_servico WHERE id_os_servico = @IdOsServico";

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await ExecuteAsync(sql, new { servico.IdOsServico }, ct);
                await ExecuteAsync(RecalcularValorOs, new { servico.IdOs }, ct);

                return true;
            }, cancellationToken);
        }

        public async Task AtualizarQuantidadeServicoAsync(OrdemServicoServico servico, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE os_servico SET quantidade = @Quantidade, valor_cobrado = @ValorCobrado WHERE id_os_servico = @IdOsServico";

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await ExecuteAsync(sql, new { servico.IdOsServico, servico.Quantidade, servico.ValorCobrado }, ct);
                await ExecuteAsync(RecalcularValorOs, new { servico.IdOs }, ct);

                return true;
            }, cancellationToken);
        }

        public async Task<IReadOnlyList<OrdemServicoInsumo>> ObterInsumosAsync(int idOs, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT id_os_insumo, id_os, id_insumo, quantidade, valor_cobrado FROM os_insumo WHERE id_os = @IdOs";

            var rows = await QueryAsync<OsInsumoRow>(sql, new { IdOs = idOs }, cancellationToken);

            return rows.Select(Reconstituir).ToList();
        }

        public async Task<OrdemServicoInsumo> AdicionarInsumoAsync(OrdemServicoInsumo insumo, CancellationToken cancellationToken = default)
        {
            const string sql = "INSERT INTO os_insumo (id_os, id_insumo, quantidade, valor_cobrado) VALUES (@IdOs, @IdInsumo, @Quantidade, @ValorCobrado) RETURNING id_os_insumo";
            try
            {
                int idOsInsumo = default;
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    idOsInsumo = await ExecuteScalarAsync<int>(sql, new
                    {
                        insumo.IdOs,
                        insumo.IdInsumo,
                        insumo.Quantidade,
                        insumo.ValorCobrado
                    }, ct);

                    await ExecuteAsync(RecalcularValorOs, new { insumo.IdOs }, ct);

                    return true;
                }, cancellationToken);

                return OrdemServicoInsumo.Reconstituir(idOsInsumo, insumo.IdOs, insumo.IdInsumo, insumo.Quantidade, insumo.ValorCobrado);
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException(MensagemReferenciaInvalida(ex));
            }
        }

        public async Task RemoverInsumoAsync(OrdemServicoInsumo insumo, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM os_insumo WHERE id_os_insumo = @IdOsInsumo";

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await ExecuteAsync(sql, new { insumo.IdOsInsumo }, ct);
                await ExecuteAsync(RecalcularValorOs, new { insumo.IdOs }, ct);

                return true;
            }, cancellationToken);
        }

        public async Task AtualizarQuantidadeInsumoAsync(OrdemServicoInsumo insumo, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE os_insumo SET quantidade = @Quantidade, valor_cobrado = @ValorCobrado WHERE id_os_insumo = @IdOsInsumo";

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await ExecuteAsync(sql, new { insumo.IdOsInsumo, insumo.Quantidade, insumo.ValorCobrado }, ct);
                await ExecuteAsync(RecalcularValorOs, new { insumo.IdOs }, ct);

                return true;
            }, cancellationToken);
        }

        private static string MensagemReferenciaInvalida(PostgresException ex) => ex.ConstraintName switch
        {
            "fk_os_veiculo" => "Veículo não encontrado.",
            "fk_os_status_colaborador" => "Colaborador não encontrado.",
            "fk_os_status_status" => "Status não encontrado.",
            "fk_os_servico_servico" => "Serviço não encontrado.",
            "fk_os_insumo_insumo" => "Insumo não encontrado.",
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

        private static OrdemServicoServico Reconstituir(OsServicoRow row) => OrdemServicoServico.Reconstituir(
            row.IdOsServico,
            row.IdOs,
            row.IdServico,
            row.Quantidade,
            row.ValorCobrado
        );

        private static OrdemServicoInsumo Reconstituir(OsInsumoRow row) => OrdemServicoInsumo.Reconstituir(
            row.IdOsInsumo,
            row.IdOs,
            row.IdInsumo,
            row.Quantidade,
            row.ValorCobrado
        );
    }
}
