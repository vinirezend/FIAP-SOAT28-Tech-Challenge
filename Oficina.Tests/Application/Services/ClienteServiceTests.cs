using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Implementations.Services;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces.Repositories;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.Tests.Services
{
    public class ClienteServiceTests
    {
        // Valores válidos para os Value Objects (ajuste se suas validações forem diferentes)
        private const string Documento1 = "32887790056"; // CPF válido
        private const string Email1 = "lucas.ferreira@example.com";
        private const string Telefone1 = "1933333333";

        private readonly Mock<IClienteRepository> _repositoryMock = new();
        private readonly ClienteService _sut;
        private readonly CancellationToken _ct = new CancellationTokenSource().Token;

        public ClienteServiceTests()
        {
            _sut = new ClienteService(_repositoryMock.Object);
        }

        #region Helpers

        private static ClienteDTO CriarDto(
            int id = 0,
            string nome = "Lucas Ferreira",
            string documento = Documento1,
            string email = Email1,
            string telefone = Telefone1) => new()
            {
                IdCliente = id,
                Nome = nome,
                Documento = documento,
                Email = email,
                Telefone = telefone
            };

        private static Cliente CriarEntidade(string nome = "Lucas Ferreira") =>
            Cliente.Criar(
                nome,
                Documento.Criar(Documento1),
                Email.Criar(Email1),
                Telefone.Criar(Telefone1));

        #endregion

        #region AdicionarAsync

        [Fact]
        public async Task AdicionarAsync_DadosValidos_DevePersistirERetornarDto()
        {
            var dto = CriarDto();
            _repositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Cliente>(), _ct))
                .ReturnsAsync((Cliente c, CancellationToken _) => c);

            var result = await _sut.AdicionarAsync(dto, _ct);

            Assert.NotNull(result);
            Assert.Equal(dto.Nome, result.Nome);
            Assert.Equal(dto.Documento, result.Documento);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(dto.Telefone, result.Telefone);
            Assert.False(string.IsNullOrWhiteSpace(result.TipoDocumento));

            _repositoryMock.Verify(r => r.AdicionarAsync(
                It.Is<Cliente>(c => c.Nome == dto.Nome && c.Documento.Numero == dto.Documento),
                _ct), Times.Once);
        }

        [Theory]
        [InlineData("123", Email1, Telefone1)]              // documento inválido
        [InlineData(Documento1, "email-invalido", Telefone1)] // e-mail inválido
        [InlineData(Documento1, Email1, "abc")]             // telefone inválido
        public async Task AdicionarAsync_ValueObjectInvalido_DeveLancarExcecaoENaoPersistir(
            string documento, string email, string telefone)
        {
            var dto = CriarDto(documento: documento, email: email, telefone: telefone);

            await Assert.ThrowsAnyAsync<Exception>(() => _sut.AdicionarAsync(dto, _ct));

            _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_ClienteExistente_DeveAtualizarPersistirERetornarDto()
        {
            var entidade = CriarEntidade("Nome Antigo");
            var dto = CriarDto(id: 1, nome: "Nome Novo", email: "novo@email.com", telefone: "11912345678");
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1, _ct)).ReturnsAsync(entidade);

            var result = await _sut.AtualizarAsync(dto, _ct);

            Assert.NotNull(result);
            Assert.Equal("Nome Novo", result!.Nome);
            Assert.Equal("novo@email.com", result.Email);
            Assert.Equal("11912345678", result.Telefone);
            _repositoryMock.Verify(r => r.AtualizarAsync(entidade, _ct), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_ClienteInexistente_DeveRetornarNullENaoAtualizar()
        {
            _repositoryMock.Setup(r => r.ObterPorIdAsync(99, _ct)).ReturnsAsync((Cliente?)null);

            var result = await _sut.AtualizarAsync(CriarDto(id: 99), _ct);

            Assert.Null(result);
            _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarAsync_EmailInvalido_DeveLancarExcecaoENaoAtualizar()
        {
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1, _ct)).ReturnsAsync(CriarEntidade());
            var dto = CriarDto(id: 1, email: "email-invalido");

            await Assert.ThrowsAnyAsync<Exception>(() => _sut.AtualizarAsync(dto, _ct));

            _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarAsync_TelefoneInvalido_DeveLancarExcecaoENaoAtualizar()
        {
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1, _ct)).ReturnsAsync(CriarEntidade());
            var dto = CriarDto(id: 1, telefone: "abc");

            await Assert.ThrowsAnyAsync<Exception>(() => _sut.AtualizarAsync(dto, _ct));

            _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_ClienteExistente_DeveRetornarDto()
        {
            var entidade = CriarEntidade();
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1, _ct)).ReturnsAsync(entidade);

            var result = await _sut.ObterPorIdAsync(1, _ct);

            Assert.NotNull(result);
            Assert.Equal(entidade.IdCliente, result!.IdCliente);
            Assert.Equal(entidade.Nome, result.Nome);
            Assert.Equal(entidade.Documento.Numero, result.Documento);
            Assert.Equal(entidade.Documento.Tipo.ToString(), result.TipoDocumento);
            Assert.Equal(entidade.Email.Endereco, result.Email);
            Assert.Equal(entidade.Telefone.Numero, result.Telefone);
        }

        [Fact]
        public async Task ObterPorIdAsync_ClienteInexistente_DeveRetornarNull()
        {
            _repositoryMock.Setup(r => r.ObterPorIdAsync(99, _ct)).ReturnsAsync((Cliente?)null);

            var result = await _sut.ObterPorIdAsync(99, _ct);

            Assert.Null(result);
        }

        #endregion

        #region ObterPorDocumentoAsync

        [Fact]
        public async Task ObterPorDocumentoAsync_ClienteExistente_DeveRetornarDto()
        {
            var entidade = CriarEntidade();
            _repositoryMock.Setup(r => r.ObterPorDocumentoAsync(Documento1, _ct)).ReturnsAsync(entidade);

            var result = await _sut.ObterPorDocumentoAsync(Documento1, _ct);

            Assert.NotNull(result);
            Assert.Equal(entidade.Nome, result!.Nome);
            Assert.Equal(entidade.Documento.Numero, result.Documento);
        }

        [Fact]
        public async Task ObterPorDocumentoAsync_ClienteInexistente_DeveRetornarNull()
        {
            _repositoryMock.Setup(r => r.ObterPorDocumentoAsync("00000000000", _ct)).ReturnsAsync((Cliente?)null);

            var result = await _sut.ObterPorDocumentoAsync("00000000000", _ct);

            Assert.Null(result);
        }

        #endregion

        #region ObterTodosAsync

        [Fact]
        public async Task ObterTodosAsync_ComClientes_DeveRetornarTodosMapeados()
        {
            var entidades = new List<Cliente> { CriarEntidade("Cliente 1"), CriarEntidade("Cliente 2") };
            _repositoryMock.Setup(r => r.ObterTodosAsync(_ct)).ReturnsAsync(entidades);

            var result = await _sut.ObterTodosAsync(_ct);

            Assert.Equal(2, result.Count);
            Assert.Equal(entidades.Select(e => e.Nome), result.Select(r => r.Nome));
        }

        [Fact]
        public async Task ObterTodosAsync_SemClientes_DeveRetornarListaVazia()
        {
            _repositoryMock.Setup(r => r.ObterTodosAsync(_ct)).ReturnsAsync(new List<Cliente>());

            var result = await _sut.ObterTodosAsync(_ct);

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
