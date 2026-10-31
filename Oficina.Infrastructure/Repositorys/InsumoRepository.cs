using Dapper;
using Microsoft.Extensions.Configuration;
using Oficina.Application.Exceptions;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Infrastructure.Models;


namespace Oficina.Infrastructure.Repositorys
{
    public class InsumoRepository : BaseRepository, IInsumoRepository
    {
        public InsumoRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<Insumo> AdicionarAsync(Insumo insumo, CancellationToken cancellationToken = default)
        {
            const string sql = $"INSERT INTO insumo (nome, descricao, ativo, quantidade_estoque, quantidade_reservada,valor_unitario) VALUES (@Nome, @Descricao, @Ativo, @QuantidadeEstoque, @QuantidadeReservada, @ValorUnitario ) RETURNING id_insumo";

            var idInsumo = await ExecuteScalarAsync<int>(sql, new
            {
                insumo.Nome,
                insumo.Descricao,
                insumo.Ativo,
                insumo.QuantidadeEstoque,
                insumo.QuantidadeReservada,
                insumo.ValorUnitario
            }, cancellationToken);

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

        public async Task AtualizarAsync(Insumo insumo, CancellationToken cancellationToken = default)
        {
            const string sql = $"UPDATE insumo SET nome = @Nome, descricao = @Descricao, ativo = @Ativo, quantidade_estoque = @QuantidadeEstoque, quantidade_reservada = @QuantidadeReservada, valor_unitario = @ValorUnitario WHERE id_insumo = @IdInsumo";
            await ExecuteAsync(sql, new
            {
                insumo.Nome,
                insumo.Descricao,
                insumo.Ativo,
                insumo.QuantidadeEstoque,
                insumo.QuantidadeReservada,
                insumo.ValorUnitario,
                insumo.IdInsumo
            }, cancellationToken);
        }

        public async Task<Insumo?> ObterPorIdAsync(int idInsumo, CancellationToken cancellationToken = default)
        {
            const string sql = $"SELECT * FROM insumo WHERE id_insumo = @IdInsumo";
            var row = await QueryFirstOrDefaultAsync<InsumoRow>(sql, new { IdInsumo = idInsumo }, cancellationToken);

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

        public async Task<IReadOnlyList<Insumo>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            const string sql = $"SELECT * FROM insumo";

            var rows = await QueryAsync<InsumoRow>(sql, cancellationToken: cancellationToken);

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

        public async Task<IReadOnlyList<Insumo>?> PesquisarAsync(string? termo, CancellationToken cancellationToken = default)
        {
            const string sql = $"SELECT * FROM insumo WHERE nome ILIKE @Termo OR descricao ILIKE @Termo";

            var rows = await QueryAsync<InsumoRow>(sql, new { Termo = $"%{termo}%" }, cancellationToken);

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

        public async Task RemoverAsync(int idInsumo, CancellationToken cancellationToken = default)
        {
            const string sql = $"DELETE FROM insumo WHERE id_insumo = @IdInsumo";
            await ExecuteAsync(sql, new { IdInsumo = idInsumo }, cancellationToken);
        }

        public async Task<int> ObterQuantidadeEstoqueAsync(int idInsumo, CancellationToken cancellationToken = default)
        {
            string sql = @"SELECT 
                                quantidade_estoque
                            FROM insumo 
                            WHERE id_insumo = @idInsumo;";

            return await QueryFirstOrDefaultAsync<int>(sql, new { idInsumo }, cancellationToken);
        }

        public async Task<int> ObterQuantidadeDisponivelAsync(int idInsumo, CancellationToken cancellationToken = default)
        {
            string sql = @"SELECT 
                                quantidade_estoque - quantidade_reservada 
                            FROM insumo 
                            WHERE id_insumo = @idInsumo;";

            return await QueryFirstOrDefaultAsync<int>(sql, new { idInsumo }, cancellationToken);
        }

        public async Task<int> ObterQuantidadeReservadaAsync(int idInsumo, CancellationToken cancellationToken = default)
        {
            string sql = @"SELECT 
                                quantidade_reservada 
                            FROM insumo 
                            WHERE id_insumo = @idInsumo;";

            return await QueryFirstOrDefaultAsync<int>(sql, new { idInsumo }, cancellationToken);
        }

        public async Task AdicionarEstoqueAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default)
        {
            string sql = @"UPDATE insumo
                                    SET quantidade_estoque = quantidade_estoque + @quantidade
                                    WHERE id_insumo = @idInsumo;";

            var retorno = await ExecuteAsync(sql, new { idInsumo, quantidade }, cancellationToken);

            if (retorno == 0)
                throw new BusinessException("Insumo não localizado.");
        }

        public async Task RetirarEstoqueAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default)
        {
            string sql = @"UPDATE insumo
                                SET quantidade_estoque = quantidade_estoque - @quantidade
                                WHERE id_insumo = @idInsumo
                                    AND quantidade_estoque - quantidade_reservada >= @quantidade;";

            var retorno = await ExecuteAsync(sql, new { idInsumo, quantidade }, cancellationToken);

            if (retorno == 0)
                throw new BusinessException(
                    "Insumo não localizado ou quantidade disponível insuficiente em estoque.");
        }

        public async Task AdicionarReservaAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default)
        {
            string sql = @"UPDATE insumo
                                    SET quantidade_reservada = quantidade_reservada + @quantidade
                                    WHERE id_insumo = @idInsumo
                                      AND quantidade_estoque - quantidade_reservada >= @quantidade;";

            var retorno = await ExecuteAsync(sql, new { idInsumo, quantidade }, cancellationToken);

            if (retorno == 0)
                throw new BusinessException(
                    "Insumo não localizado ou quantidade disponível insuficiente para reserva.");
        }

        public async Task RetirarReservaAsync(int idInsumo, int quantidade, CancellationToken cancellationToken = default)
        {
            string sql = @"UPDATE insumo
                                    SET quantidade_reservada = quantidade_reservada - @quantidade
                                    WHERE id_insumo = @idInsumo
                                      AND quantidade_reservada >= @quantidade;";

            var retorno = await ExecuteAsync(sql, new { idInsumo, quantidade }, cancellationToken);

            if (retorno == 0)
                throw new BusinessException(
                    "Insumo não localizado ou quantidade reservada insuficiente para retirada.");
        }
    }
}