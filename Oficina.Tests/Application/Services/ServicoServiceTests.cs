using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Implementations.Services;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces.Repositories;

namespace Oficina.Application.Tests.Services
{
    public class ServicoServiceTests
    {
        private readonly Mock<IServicoRepository> _repositoryMock = new();
        private readonly ServicoService _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public ServicoServiceTests()
        {
            _sut = new ServicoService(_repositoryMock.Object);
        }

        #region Helpers

        private static ServicoDTO CriarDto(int id = 0) => new()
        {
            IdServico = id,
            Nome = "Troca de óleo",
            Descricao = "Troca de óleo e filtro",
            ValorEstipulado = 150m,
            Ativo = true
        };

        private static Servico CriarEntidade(string nome = "Troca de óleo") =>
            Servico.Criar(nome, "Troca de óleo e filtro", 150m);

        #endregion

        #region AdicionarAsync

        [Fact]
        public async Task AdicionarAsync_DadosValidos_DevePersistirERetornarDto()
        {
            var dto = CriarDto();
            _repositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Servico>(), _ct))
                .ReturnsAsync((Servico s, CancellationToken _) => s);

            var result = await _sut.AdicionarAsync(dto, _ct);

            Assert.Equal(dto.Nome, result.Nome);
            Assert.Equal(dto.Descricao, result.Descricao);
            Assert.Equal(dto.ValorEstipulado, result.ValorEstipulado);

            _repositoryMock.Verify(r => r.AdicionarAsync(
                It.Is<Servico>(s => s.Nome == dto.Nome
                                 && s.Descricao == dto.Descricao
                                 && s.ValorEstipulado == dto.ValorEstipulado),
                _ct), Times.Once);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_ServicoExistente_DeveAtualizarPersistirERetornarDto()
        {
            var entidade = CriarEntidade("Nome Antigo");
            var dto = CriarDto(id: 1);
            dto.Nome = "Nome Novo";
            dto.Descricao = "Descrição nova";
            dto.ValorEstipulado = 200m;
            dto.Ativo = false;
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1, _ct)).ReturnsAsync(entidade);

            var result = await _sut.AtualizarAsync(dto, _ct);

            Assert.NotNull(result);
            Assert.Equal("Nome Novo", result!.Nome);
            Assert.Equal("Descrição nova", result.Descricao);
            Assert.Equal(200m, result.ValorEstipulado);
            Assert.False(result.Ativo);
            _repositoryMock.Verify(r => r.AtualizarAsync(entidade, _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_ServicoInexistente_DeveRetornarNullENaoAtualizar()
        {
            _repositoryMock.Setup(r => r.ObterPorIdAsync(99, _ct)).ReturnsAsync((Servico?)null);

            var result = await _sut.AtualizarAsync(CriarDto(id: 99), _ct);

            Assert.Null(result);
            _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Servico>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_ServicoExistente_DeveRetornarDto()
        {
            var entidade = CriarEntidade();
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1, _ct)).ReturnsAsync(entidade);

            var result = await _sut.ObterPorIdAsync(1, _ct);

            Assert.NotNull(result);
            Assert.Equal(entidade.IdServico, result!.IdServico);
            Assert.Equal(entidade.Nome, result.Nome);
            Assert.Equal(entidade.Descricao, result.Descricao);
            Assert.Equal(entidade.ValorEstipulado, result.ValorEstipulado);
            Assert.Equal(entidade.Ativo, result.Ativo);
        }

        [Fact]
        public async Task ObterPorIdAsync_ServicoInexistente_DeveRetornarNull()
        {
            _repositoryMock.Setup(r => r.ObterPorIdAsync(99, _ct)).ReturnsAsync((Servico?)null);

            var result = await _sut.ObterPorIdAsync(99, _ct);

            Assert.Null(result);
        }

        #endregion

        #region ObterTodosAsync

        [Fact]
        public async Task ObterTodosAsync_ComServicos_DeveRetornarTodosMapeados()
        {
            var entidades = new List<Servico> { CriarEntidade("Serviço 1"), CriarEntidade("Serviço 2") };
            _repositoryMock.Setup(r => r.ObterTodosAsync(_ct)).ReturnsAsync(entidades);

            var result = await _sut.ObterTodosAsync(_ct);

            Assert.Equal(2, result.Count);
            Assert.Equal(entidades.Select(e => e.Nome), result.Select(r => r.Nome));
        }

        [Fact]
        public async Task ObterTodosAsync_SemServicos_DeveRetornarListaVazia()
        {
            _repositoryMock.Setup(r => r.ObterTodosAsync(_ct)).ReturnsAsync(new List<Servico>());

            var result = await _sut.ObterTodosAsync(_ct);

            Assert.Empty(result);
        }

        #endregion

        #region PesquisarAsync

        [Fact]
        public async Task PesquisarAsync_ComResultados_DeveRetornarDtosMapeados()
        {
            var entidades = new List<Servico> { CriarEntidade("Troca A"), CriarEntidade("Troca B") };
            _repositoryMock.Setup(r => r.PesquisarAsync("troca", _ct)).ReturnsAsync(entidades);

            var result = await _sut.PesquisarAsync("troca", _ct);

            Assert.Equal(2, result.Count);
            Assert.Equal(entidades.Select(e => e.Nome), result.Select(r => r.Nome));
        }

        [Fact]
        public async Task PesquisarAsync_TermoNulo_DeveRepassarNuloAoRepositorio()
        {
            var entidades = new List<Servico> { CriarEntidade() };
            _repositoryMock.Setup(r => r.PesquisarAsync(null, _ct)).ReturnsAsync(entidades);

            var result = await _sut.PesquisarAsync(null, _ct);

            Assert.Single(result);
            _repositoryMock.Verify(r => r.PesquisarAsync(null, _ct), Times.Once);
        }

        [Fact]
        public async Task PesquisarAsync_RepositorioRetornaNull_DeveRetornarListaVazia()
        {
            _repositoryMock.Setup(r => r.PesquisarAsync("x", _ct)).ReturnsAsync((IReadOnlyList<Servico>?)null!);

            var result = await _sut.PesquisarAsync("x", _ct);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task PesquisarAsync_SemResultados_DeveRetornarListaVazia()
        {
            _repositoryMock.Setup(r => r.PesquisarAsync("x", _ct)).ReturnsAsync(new List<Servico>());

            var result = await _sut.PesquisarAsync("x", _ct);

            Assert.Empty(result);
        }

        #endregion

        #region RemoverAsync

        [Fact]
        public async Task RemoverAsync_DeveDelegarParaRepositorio()
        {
            await _sut.RemoverAsync(5, _ct);

            _repositoryMock.Verify(r => r.RemoverAsync(5, _ct), Times.Once);
        }

        #endregion
    }
}