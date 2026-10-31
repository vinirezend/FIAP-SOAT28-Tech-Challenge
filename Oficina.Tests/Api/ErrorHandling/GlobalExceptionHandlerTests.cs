using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Oficina.Api.ErrorHandling;
using Oficina.Application.Exceptions;

namespace Oficina.Tests.Api.ErrorHandling;

public class GlobalExceptionHandlerTests
{
    private readonly IServiceProvider _services = new ServiceCollection()
        .AddLogging()
        .AddProblemDetails()
        .BuildServiceProvider();

    private GlobalExceptionHandler CriarHandler() =>
        new(_services.GetRequiredService<IProblemDetailsService>(), NullLogger<GlobalExceptionHandler>.Instance);

    private DefaultHttpContext CriarContexto() => new()
    {
        RequestServices = _services,
        Response = { Body = new MemoryStream() }
    };

    private static async Task<JsonElement> LerCorpoAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return (await JsonDocument.ParseAsync(context.Response.Body)).RootElement;
    }

    // Type e string são serializáveis, então o Test Explorer lista cada caso separadamente
    [Theory]
    [InlineData(typeof(BusinessException), "Regra de negócio violada.")]
    [InlineData(typeof(ArgumentException), "CPF/CNPJ inválido.")]
    [InlineData(typeof(InvalidOperationException), "Status não permite a operação.")]
    public async Task TryHandleAsync_ExcecaoDeNegocio_DeveRetornar400ComAMensagem(Type tipoExcecao, string mensagem)
    {
        var excecao = (Exception)Activator.CreateInstance(tipoExcecao, mensagem)!;
        var context = CriarContexto();

        var tratado = await CriarHandler().TryHandleAsync(context, excecao, CancellationToken.None);

        Assert.True(tratado);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);

        var corpo = await LerCorpoAsync(context);
        Assert.Equal(400, corpo.GetProperty("status").GetInt32());
        Assert.Equal(excecao.Message, corpo.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_ExcecaoInesperada_DeveRetornar500SemExporDetalhes()
    {
        var context = CriarContexto();

        var tratado = await CriarHandler().TryHandleAsync(context, new NullReferenceException("detalhe interno"), CancellationToken.None);

        Assert.True(tratado);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var corpo = await LerCorpoAsync(context);
        Assert.Equal(GlobalExceptionHandler.MensagemErroInterno, corpo.GetProperty("detail").GetString());
        Assert.DoesNotContain("detalhe interno", corpo.GetRawText());
    }

    [Fact]
    public async Task TryHandleAsync_RequisicaoCanceladaPeloCliente_NaoDeveEscreverResposta()
    {
        using var cancelamento = new CancellationTokenSource();
        var context = CriarContexto();
        context.RequestAborted = cancelamento.Token;
        await cancelamento.CancelAsync();

        var tratado = await CriarHandler().TryHandleAsync(context, new OperationCanceledException(), CancellationToken.None);

        Assert.True(tratado);
        Assert.Equal(0, context.Response.Body.Length);
    }
}
