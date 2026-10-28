using Moq;
using Oficina.Application.Implementations.Services;
using Oficina.Domain.Entitys;
using Oficina.Domain.Interfaces.Repositorys;

namespace Oficina.Tests.Application.Services;

public class StatusServiceTests
{
    private readonly Mock<IStatusRepository> _repositoryMock = new();
    private readonly StatusService _service;

    public StatusServiceTests()
    {
        _service = new StatusService(_repositoryMock.Object);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarStatus()
    {
        _repositoryMock
            .Setup(r => r.ObterTodosAsync())
            .ReturnsAsync([Status.Reconstituir(1, "Recebida"), Status.Reconstituir(2, "Em diagnóstico")]);

        var resultado = await _service.ObterTodosAsync();

        Assert.Equal(["Recebida", "Em diagnóstico"], resultado.Select(s => s.Nome));
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarStatus()
    {
        _repositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(Status.Reconstituir(1, "Recebida"));

        var resultado = await _service.ObterPorIdAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal("Recebida", resultado.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_StatusInexistente_DeveRetornarNull()
    {
        var resultado = await _service.ObterPorIdAsync(99);

        Assert.Null(resultado);
    }
}
