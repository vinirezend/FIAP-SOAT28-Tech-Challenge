using Moq;
using Oficina.Application.DTOs;
using Oficina.Application.Implementations.Services;
using Oficina.Domain.Entitys;
using Oficina.Domain.Enum;
using Oficina.Domain.Interfaces.Repositorys;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Application.Services;

public class ColaboradorServiceTests
{
    private readonly Mock<IColaboradorRepository> _repositoryMock = new();
    private readonly ColaboradorService _service;

    public ColaboradorServiceTests()
    {
        _service = new ColaboradorService(_repositoryMock.Object);
    }

    private static Colaborador CriarColaborador() => Colaborador.Reconstituir(
        1, "Oficina 1", Documento.Criar("50118487442"), Email.Criar("oficina@oficina.com"), true, Departamento.Oficina, 3);

    [Fact]
    public async Task AdicionarAsync_DeveCriarColaboradorAtivo()
    {
        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Colaborador>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Colaborador c, CancellationToken _) => Colaborador.Reconstituir(7, c.Nome, c.Cpf, c.Email, c.Ativo, c.IdDepartamento, c.IdUsuario));

        var resultado = await _service.AdicionarAsync(new ColaboradorDTO
        {
            Nome = "Novo",
            Cpf = "029.947.066-01",
            Email = "Novo@Oficina.com",
            IdDepartamento = (int)Departamento.Recepcao
        });

        Assert.Equal(7, resultado.IdColaborador);
        Assert.Equal("02994706601", resultado.Cpf);
        Assert.Equal("novo@oficina.com", resultado.Email);
        Assert.True(resultado.Ativo);
    }

    [Fact]
    public async Task AdicionarAsync_CpfInvalido_DeveLancarExcecao()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AdicionarAsync(new ColaboradorDTO
        {
            Nome = "Novo",
            Cpf = "11111111111",
            Email = "novo@oficina.com",
            IdDepartamento = (int)Departamento.Recepcao
        }));
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarSemAlterarCpf()
    {
        _repositoryMock.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CriarColaborador());

        var resultado = await _service.AtualizarAsync(new ColaboradorDTO
        {
            IdColaborador = 1,
            Nome = "Oficina 2",
            Cpf = "02994706601",
            Email = "oficina2@oficina.com",
            IdDepartamento = (int)Departamento.Oficina,
            Ativo = false
        });

        Assert.NotNull(resultado);
        Assert.Equal("Oficina 2", resultado.Nome);
        Assert.Equal("50118487442", resultado.Cpf);
        Assert.False(resultado.Ativo);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Colaborador>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_ColaboradorInexistente_DeveRetornarNull()
    {
        var resultado = await _service.AtualizarAsync(new ColaboradorDTO { IdColaborador = 99 });

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarColaboradores()
    {
        _repositoryMock.Setup(r => r.ObterTodosAsync(It.IsAny<CancellationToken>())).ReturnsAsync([CriarColaborador()]);

        var resultado = await _service.ObterTodosAsync();

        Assert.Equal("Oficina 1", Assert.Single(resultado).Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarColaborador()
    {
        _repositoryMock.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CriarColaborador());

        var resultado = await _service.ObterPorIdAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal((int)Departamento.Oficina, resultado.IdDepartamento);
        Assert.Equal(3, resultado.IdUsuario);
    }

    [Fact]
    public async Task RemoverAsync_DeveRemover()
    {
        await _service.RemoverAsync(1);

        _repositoryMock.Verify(r => r.RemoverAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
