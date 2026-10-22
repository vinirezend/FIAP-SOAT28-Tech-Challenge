using Dapper;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Microsoft.Extensions.Configuration;
using Oficina.Domain.ValueObjects;
using Oficina.Domain.Enum;
using Oficina.Infrastructure.Models;
using Npgsql;



namespace Oficina.Infrastructure.Repositorys
{
    public class VeiculoRepository : BaseRepository, IVeiculoRepository
    {
        public VeiculoRepository(IConfiguration configuration) : base(configuration) { }
        public async Task<Veiculo> AdicionarAsync(Veiculo veiculo)
        {
            const string sql = $"INSERT INTO veiculo (placa, marca, modelo, ano, id_tipo_veiculo, id_cliente) VALUES (@Placa, @Marca, @Modelo, @Ano, @IdTipoVeiculo, @IdCliente) RETURNING id_veiculo";
            try
            {
                using (var connection = await _dataSource.OpenConnectionAsync())
                {
                    var idVeiculo = await connection.ExecuteScalarAsync<int>(sql, new
                    {
                        Placa = veiculo.Placa.Numero,
                        veiculo.IdCliente,
                        veiculo.Marca,
                        veiculo.Modelo,
                        veiculo.Ano,
                        veiculo.IdTipoVeiculo

                    });

                    return Veiculo.Reconstituir(idVeiculo, veiculo.IdCliente, veiculo.Placa, veiculo.Modelo, veiculo.Marca, veiculo.Ano, veiculo.IdTipoVeiculo);
                }
            }
            catch (PostgresException ex) when (ex.SqlState is "23505")
            {
                throw new InvalidOperationException("Ja existe um veiculo com essa placa.");
            }
        }
        public async Task AtualizarAsync(Veiculo veiculo)
        {
            const string sql = $"UPDATE veiculo SET id_cliente = @IdCliente, placa = @Placa, marca = @Marca, modelo = @Modelo, ano = @Ano, id_tipo_veiculo = @IdTipoVeiculo WHERE id_veiculo = @IdVeiculo";
            try
            {
                using (var connection = await _dataSource.OpenConnectionAsync())
                {
                    await connection.ExecuteAsync(sql, new
                    {
                        veiculo.IdVeiculo,
                        Placa = veiculo.Placa.Numero,
                        veiculo.Marca,
                        veiculo.Modelo,
                        veiculo.Ano,
                        veiculo.IdTipoVeiculo,
                        veiculo.IdCliente
                    });
                }
            }
            catch (PostgresException ex) when (ex.SqlState is "23505")
            {
                throw new InvalidOperationException("Ja existe um veiculo com essa placa.");
            }
        }

        public async Task<Veiculo?> ObterPorIdAsync(int idVeiculo)
        {
            const string sql = $"SELECT * FROM veiculo WHERE id_veiculo = @IdVeiculo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var veiculo = await connection.QueryFirstOrDefaultAsync<VeiculoRow>(sql, new { IdVeiculo = idVeiculo });
                return veiculo is null ? null : Veiculo.Reconstituir(veiculo.IdVeiculo, veiculo.IdCliente, Placa.Criar(veiculo.Placa), veiculo.Modelo, veiculo.Marca, veiculo.Ano, (TipoVeiculo)veiculo.IdTipoVeiculo);
            }
        }

        public async Task<Veiculo?> ObterPorPlacaAsync(string placa)
        {
            const string sql = $"SELECT * FROM veiculo WHERE placa = @Placa";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var veiculo = await connection.QueryFirstOrDefaultAsync<VeiculoRow>(sql, new { Placa = placa });
                return veiculo is null ? null : Veiculo.Reconstituir(veiculo.IdVeiculo, veiculo.IdCliente, Placa.Criar(veiculo.Placa), veiculo.Modelo, veiculo.Marca, veiculo.Ano, (TipoVeiculo)veiculo.IdTipoVeiculo);
            }
        }

        public async Task<IReadOnlyList<Veiculo>> ObterTodosAsync()
        {
            const string sql = $"SELECT * FROM veiculo";
            using (var connection = await _dataSource.OpenConnectionAsync())
            {
                var rows = await connection.QueryAsync<VeiculoRow>(sql);
                return rows.Select(row => Veiculo.Reconstituir(
                    row.IdVeiculo,
                     row.IdCliente,
                    Placa.Criar(row.Placa),
                    row.Modelo,
                    row.Marca,
                    row.Ano,
                    (TipoVeiculo)row.IdTipoVeiculo
                )).ToList();
            }
        }
        public async Task RemoverAsync(int idVeiculo)
        {
            const string sql = $"DELETE FROM veiculo WHERE id_veiculo = @IdVeiculo";

            try
            {
                using (var connection = await _dataSource.OpenConnectionAsync())
                {
                    await connection.ExecuteAsync(sql, new { IdVeiculo = idVeiculo });
                }
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new InvalidOperationException("Nao foi possivel remover o veiculo.");
            }
        }
    }
}