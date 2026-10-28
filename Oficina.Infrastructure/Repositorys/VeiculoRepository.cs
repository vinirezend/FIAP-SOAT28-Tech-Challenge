using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Oficina.Application.Exceptions;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.ValueObjects;
using Oficina.Infrastructure.Models;



namespace Oficina.Infrastructure.Repositorys
{
    public class VeiculoRepository : BaseRepository, IVeiculoRepository
    {
        public VeiculoRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<Veiculo> AdicionarAsync(Veiculo veiculo)
        {
            const string sql = $"INSERT INTO veiculo (placa, marca, modelo, ano, id_tipo_veiculo, id_cliente) VALUES (@Placa, @Marca, @Modelo, @Ano, @IdTipoVeiculo, @IdCliente) RETURNING id_veiculo";
            try
            {
                var idVeiculo = await ExecuteScalarAsync<int>(sql, new
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
            catch (PostgresException ex) when (ex.SqlState is "23505")
            {
                throw new BusinessException("Ja existe um veiculo com essa placa.");
            }catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new BusinessException("Cliente não encontrado.");
            }
        }

        public async Task AtualizarAsync(Veiculo veiculo)
        {
            const string sql = $"UPDATE veiculo SET id_cliente = @IdCliente, placa = @Placa, marca = @Marca, modelo = @Modelo, ano = @Ano, id_tipo_veiculo = @IdTipoVeiculo WHERE id_veiculo = @IdVeiculo";
            try
            {
                await ExecuteAsync(sql, new
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
            catch (PostgresException ex) when (ex.SqlState is "23505")
            {
                throw new BusinessException("Ja existe um veiculo com essa placa.");
            }catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new BusinessException("Cliente não encontrado.");
            }
        }

        public async Task<Veiculo?> ObterPorIdAsync(int idVeiculo)
        {
            const string sql = $"SELECT * FROM veiculo WHERE id_veiculo = @IdVeiculo";

            var veiculo = await QueryFirstOrDefaultAsync<VeiculoRow>(sql, new { IdVeiculo = idVeiculo });

            return veiculo is null ? null : Veiculo.Reconstituir(veiculo.IdVeiculo, veiculo.IdCliente, Placa.Criar(veiculo.Placa), veiculo.Modelo, veiculo.Marca, veiculo.Ano, (TipoVeiculo)veiculo.IdTipoVeiculo);
        }

        public async Task<Veiculo?> ObterPorPlacaAsync(string placa)
        {
            const string sql = $"SELECT * FROM veiculo WHERE placa = @Placa";

            var veiculo = await QueryFirstOrDefaultAsync<VeiculoRow>(sql, new { Placa = placa });

            return veiculo is null ? null : Veiculo.Reconstituir(veiculo.IdVeiculo, veiculo.IdCliente, Placa.Criar(veiculo.Placa), veiculo.Modelo, veiculo.Marca, veiculo.Ano, (TipoVeiculo)veiculo.IdTipoVeiculo);
        }

        public async Task<IReadOnlyList<Veiculo>> ObterTodosAsync()
        {
            const string sql = $"SELECT * FROM veiculo";

            var rows = await QueryAsync<VeiculoRow>(sql);

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

        public async Task RemoverAsync(int idVeiculo)
        {
            const string sql = $"DELETE FROM veiculo WHERE id_veiculo = @IdVeiculo";

            try
            {
                await ExecuteAsync(sql, new { IdVeiculo = idVeiculo });
            }
            catch (PostgresException ex) when (ex.SqlState is "23503")
            {
                throw new BusinessException("Nao foi possivel remover o veiculo.");
            }
        }
    }
}