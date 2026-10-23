using Dapper;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Microsoft.Extensions.Configuration;
using Oficina.Infrastructure.Models;


namespace Oficina.Infrastructure.Repositorys
{
    public class InsumoRepository : BaseRepository, IInsumoRepository
    {
        public InsumoRepository(IConfiguration configuration) : base(configuration) { }
        public async Task<Insumo> AdicionarAsync(Insumo insumo)
        {
            const string sql = $"INSERT INTO insumo (nome, descricao, ativo, quantidade_estoque, quantidade_reservada,valor_unitario) VALUES (@Nome, @Descricao, @Ativo, @QuantidadeEstoque, @QuantidadeReservada, @ValorUnitario ) RETURNING id_insumo";

            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var idInsumo = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    insumo.Nome,
                    insumo.Descricao,
                    insumo.Ativo,
                    insumo.QuantidadeEstoque,
                    insumo.QuantidadeReservada,
                    insumo.ValorUnitario
                });

                return Insumo.Reconstituir(
               idInsumo,
               insumo.Nome,
               insumo.Descricao,
               insumo.ValorUnitario,
               insumo.Ativo,
               insumo.QuantidadeEstoque,
               insumo.QuantidadeReservada
               );
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
                var row = await connection.QueryFirstOrDefaultAsync<InsumoRow>(sql, new { IdInsumo = idInsumo });
                if (row == null)
                    return null;

                return Insumo.Reconstituir(
                    row.IdInsumo,
                    row.Nome,
                    row.Descricao,
                    row.ValorUnitario,
                    row.Ativo,
                    row.QuantidadeEstoque,
                    row.QuantidadeReservada
                );
            }
        }
        public async Task<IReadOnlyList<Insumo>> ObterTodosAsync()
        {
            const string sql = $"SELECT * FROM insumo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var rows = await connection.QueryAsync<InsumoRow>(sql);
                return rows.Select(row => Insumo.Reconstituir(
                    row.IdInsumo,
                    row.Nome,
                    row.Descricao,
                    row.ValorUnitario,
                    row.Ativo,
                    row.QuantidadeEstoque,
                    row.QuantidadeReservada
                )).ToList();
            }
        }
        public async Task<IReadOnlyList<Insumo>?> PesquisarAsync(string? termo)
        {
            const string sql = $"SELECT * FROM insumo WHERE nome LIKE @Termo OR descricao LIKE @Termo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var rows = await connection.QueryAsync<InsumoRow>(sql, new { Termo = $"%{termo}%" });
                return rows.Select(row => Insumo.Reconstituir(
                    row.IdInsumo,
                    row.Nome,
                    row.Descricao,
                    row.ValorUnitario,
                    row.Ativo,
                    row.QuantidadeEstoque,
                    row.QuantidadeReservada
                )).ToList();
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