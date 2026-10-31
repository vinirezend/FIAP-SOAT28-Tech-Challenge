using Dapper;
using Microsoft.Extensions.Configuration;
using Oficina.Application.Interfaces.Resources;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Infrastructure.Models;

namespace Oficina.Infrastructure.Repositorys
{
    public class ServicoRepository : BaseRepository, IServicoRepository
    {
        public ServicoRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<Servico> AdicionarAsync(Servico servico, CancellationToken cancellationToken = default)
        {
            const string sql = $"INSERT INTO servico (nome, descricao, ativo, valor_estipulado) VALUES (@Nome, @Descricao, @Ativo, @ValorEstipulado) RETURNING id_servico";

            var idServico = await ExecuteScalarAsync<int>(sql, new
            {
                servico.Nome,
                servico.Descricao,
                servico.Ativo,
                servico.ValorEstipulado
            }, cancellationToken);
            return Servico.Reconstituir(idServico, servico.Nome, servico.Descricao, servico.ValorEstipulado, servico.Ativo);
        }

        public async Task AtualizarAsync(Servico servico, CancellationToken cancellationToken = default)
        {
            const string sql = $"UPDATE servico SET nome = @Nome, descricao = @Descricao, ativo = @Ativo, valor_estipulado = @ValorEstipulado WHERE id_servico = @IdServico";

            await ExecuteAsync(sql, new
            {
                servico.IdServico,
                servico.Nome,
                servico.Descricao,
                servico.Ativo,
                servico.ValorEstipulado
            }, cancellationToken);
        }

        public async Task<Servico?> ObterPorIdAsync(int idServico, CancellationToken cancellationToken = default)
        {
            const string sql = $"SELECT * FROM servico WHERE id_servico = @IdServico";

            var row = await QueryFirstOrDefaultAsync<ServicoRow>(sql, new { IdServico = idServico }, cancellationToken);

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

        public async Task<IReadOnlyList<Servico>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            const string sql = $"SELECT * FROM servico";

            var rows = await QueryAsync<ServicoRow>(sql, cancellationToken: cancellationToken);

            return rows.Select(row => Servico.Reconstituir(
                row.IdServico,
                row.Nome,
                row.Descricao,
                row.ValorEstipulado,
                row.Ativo
            )).ToList();
        }

        public async Task<IReadOnlyList<Servico>?> PesquisarAsync(string? termo, CancellationToken cancellationToken = default)
        {
            const string sql = $"SELECT * FROM servico WHERE nome ILIKE @Termo OR descricao ILIKE @Termo";

            var rows = await QueryAsync<ServicoRow>(sql, new { Termo = $"%{termo}%" }, cancellationToken);

            return rows.Select(row => Servico.Reconstituir(
                row.IdServico,
                row.Nome,
                row.Descricao,
                row.ValorEstipulado,
                row.Ativo
            )).ToList();
        }

        public async Task RemoverAsync(int idServico, CancellationToken cancellationToken = default)
        {
            const string sql = $"DELETE FROM servico WHERE id_servico = @IdServico";
             await ExecuteAsync(sql, new { IdServico = idServico }, cancellationToken);
        }
    }
}