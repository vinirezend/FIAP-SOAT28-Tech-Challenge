using Dapper;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Microsoft.Extensions.Configuration;
using Oficina.Infrastructure.Models;

namespace Oficina.Infrastructure.Repositorys
{
    public class ServicoRepository : BaseRepository, IServicoRepository
    {
        public ServicoRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<Servico> AdicionarAsync(Servico servico)
        {
            const string sql = $"INSERT INTO servico (nome, descricao, ativo, valor_estipulado) VALUES (@Nome, @Descricao, @Ativo, @ValorEstipulado) RETURNING id_servico";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var idServico = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    servico.Nome,
                    servico.Descricao,
                    servico.Ativo,
                    servico.ValorEstipulado
                });
                return Servico.Reconstituir(idServico, servico.Nome, servico.Descricao, servico.ValorEstipulado, servico.Ativo);
            }
    
        }

        public async Task AtualizarAsync(Servico servico)
        {
            const string sql = $"UPDATE servico SET nome = @Nome, descricao = @Descricao, ativo = @Ativo, valor_estipulado = @ValorEstipulado WHERE id_servico = @IdServico";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                await connection.ExecuteAsync(sql, new
                {
                    servico.IdServico,
                    servico.Nome,
                    servico.Descricao,
                    servico.Ativo,
                    servico.ValorEstipulado
                });
            }
        }

        public async Task<Servico?> ObterPorIdAsync(int idServico)
        {
            const string sql = $"SELECT * FROM servico WHERE id_servico = @IdServico";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var row = await connection.QueryFirstOrDefaultAsync<ServicoRow>(sql, new { IdServico = idServico });
                if(row == null)
                    return null;

                return Servico.Reconstituir(
                    row.IdServico,
                    row.Nome,
                    row.Descricao,
                    row.ValorEstipulado,
                    row.Ativo
                );
            }
        }

        public async Task<IReadOnlyList<Servico>> ObterTodosAsync()
        {
            const string sql = $"SELECT * FROM servico";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var rows = await connection.QueryAsync<ServicoRow>(sql);
                return rows.Select(row => Servico.Reconstituir(
                    row.IdServico,
                    row.Nome,
                    row.Descricao,
                    row.ValorEstipulado,
                    row.Ativo
                )).ToList();
            }
        }

        public async Task<IReadOnlyList<Servico>?> PesquisarAsync(string? termo)
        {
            const string sql = $"SELECT * FROM servico WHERE nome ILIKE @Termo OR descricao ILIKE @Termo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var rows = await connection.QueryAsync<ServicoRow>(sql, new { Termo = $"%{termo}%" });
                return rows.Select(row => Servico.Reconstituir(
                    row.IdServico,
                    row.Nome,
                    row.Descricao,
                    row.ValorEstipulado,
                    row.Ativo
                )).ToList();
            }
        }

        public async Task RemoverAsync(int idServico)
        {
            const string sql = $"DELETE FROM servico WHERE id_servico = @IdServico";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                await connection.ExecuteAsync(sql, new { IdServico = idServico });
            }
        }
    }
}