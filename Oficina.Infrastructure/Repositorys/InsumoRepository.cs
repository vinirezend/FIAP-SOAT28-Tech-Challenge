using Dapper;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Oficina.Infrastructure.Repositorys
{
    public class InsumoRepository : BaseRepository, IInsumoRepository
    {
        public InsumoRepository(IConfiguration configuration) : base(configuration) { }

        public async Task AdicionarAsync(Insumo insumo)
        {
            const string sql = $"INSERT INTO insumo (nome, descricao, ativo, quantidade_estoque, quantidade_reservada,valor_unitario) VALUES (@Nome, @Descricao, @Ativo, @QuantidadeEstoque, @QuantidadeReservada, @ValorUnitario )";


            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                await connection.ExecuteAsync(sql, new
                {
                    insumo.Nome,
                    insumo.Descricao,
                    insumo.Ativo,
                    insumo.QuantidadeEstoque,
                    insumo.QuantidadeReservada,
                    insumo.ValorUnitario
                });
            }
        }

        public async Task AtualizarAsync(Insumo insumo)
        {
            const string sql = $"UPDATE insumo SET nome = @Nome, descricao = @Descricao, ativo = @Ativo, quantidade_estoque = @QuantidadeEstoque, quantidade_reservada = @QuantidadeReservada, valor_unitario = @ValorUnitario WHERE id_insumo = @IdInsumo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                await connection.ExecuteAsync(sql, new
                {
                    insumo.Nome,
                    insumo.Descricao,
                    insumo.Ativo,
                    insumo.QuantidadeEstoque,
                    insumo.QuantidadeReservada,
                    insumo.ValorUnitario,
                    insumo.IdInsumo
                });
            }
        }

        public async Task<Insumo?> ObterPorIdAsync(int idInsumo)
        {
            const string sql = $"SELECT * FROM insumo WHERE id_insumo = @IdInsumo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                return await connection.QueryFirstOrDefaultAsync<Insumo>(sql, new { IdInsumo = idInsumo });
            }
        }

        public async Task<IReadOnlyList<Insumo>> ObterTodosAsync()
        {
            const string sql = $"SELECT * FROM insumo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                return (await connection.QueryAsync<Insumo>(sql)).AsList();
            }
        }

        public async Task<IReadOnlyList<Insumo>?> PesquisarAsync(string? termo)
        {
            const string sql = $"SELECT * FROM insumo WHERE nome LIKE @Termo OR descricao LIKE @Termo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                return (await connection.QueryAsync<Insumo>(sql, new { Termo = $"%{termo}%" })).AsList();
            }
        }

        public async Task RemoverAsync(int idInsumo)
        {
            const string sql = $"DELETE FROM insumo WHERE id_insumo = @IdInsumo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                await connection.ExecuteAsync(sql, new { IdInsumo = idInsumo });
            }
        }
    }
}