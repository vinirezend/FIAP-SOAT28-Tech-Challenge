using Microsoft.AspNetCore.Diagnostics;
using Oficina.Application.Exceptions;

namespace Oficina.Api.ErrorHandling;

/// <summary>
/// Converte as exceções não tratadas em ProblemDetails (RFC 9457), com o mesmo formato em toda a API.
/// Regras de negócio e validações do domínio viram 400; o restante vira 500 sem expor detalhes internos.
/// </summary>
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public const string MensagemErroInterno = "Ocorreu um erro interno no servidor.";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Cliente desistiu da requisição: não há para quem responder nem o que registrar como erro
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            return true;

        var (statusCode, titulo, detalhe) = exception switch
        {
            BusinessException or ArgumentException or InvalidOperationException =>
                (StatusCodes.Status400BadRequest, "Requisição inválida", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno", MensagemErroInterno)
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Erro inesperado durante {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogWarning("Erro de negócio durante {Method} {Path}: {Message}", httpContext.Request.Method, httpContext.Request.Path, exception.Message);

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new()
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = statusCode, Title = titulo, Detail = detalhe }
        });
    }
}
