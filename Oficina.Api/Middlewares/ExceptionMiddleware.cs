using Oficina.Application.Exceptions;
using System.Text.Json;

namespace Oficina.Api.Middlewares
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(
                    "Erro de negócio durante {Method} {Path}: {Message}",
                    context.Request.Method,
                    context.Request.Path,
                    ex.Message);

                context.Response.StatusCode =
                    StatusCodes.Status400BadRequest;

                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        status = context.Response.StatusCode,
                        message = ex.Message
                    }));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro inesperado durante {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                if (context.Response.HasStarted)
                    throw;

                context.Response.StatusCode =
                    StatusCodes.Status500InternalServerError;

                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        status = context.Response.StatusCode,
                        message = "Ocorreu um erro interno no servidor."
                    }));
            }
        }
    }
}
