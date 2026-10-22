using Dapper;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Oficina.Infrastructure.Repositorys
{
    public class ServicoRepository : BaseRepository, IServicoRepository
    {
        public ServicoRepository(IConfiguration configuration) : base(configuration) { }

        public async Task AdicionarAsync(Servico servico)
        {
                const string sql = $"INSERT INTO servico (nome, descricao, ativo, valor_estipulado) VALUES (@Nome, @Descricao, @Ativo, @ValorEstipulado)";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                await connection.ExecuteAsync(sql, new
                {
                    servico.Nome,
                    servico.Descricao,
                    servico.Ativo,
                    servico.ValorEstipulado
                });
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
                return await connection.QueryFirstOrDefaultAsync<Servico>(sql, new { IdServico = idServico });
            }
        }

        public async Task<IReadOnlyList<Servico>> ObterTodosAsync()
        {
            const string sql = $"SELECT * FROM servico";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                return (await connection.QueryAsync<Servico>(sql)).AsList();
            }
        }

        public async Task<IReadOnlyList<Servico>?> PesquisarAsync(string? termo)
        {
            const string sql = $"SELECT * FROM servico WHERE nome LIKE @Termo OR descricao LIKE @Termo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                return (await connection.QueryAsync<Servico>(sql, new { Termo = $"%{termo}%" })).AsList();
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