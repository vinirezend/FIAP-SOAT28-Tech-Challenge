using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Oficina.Api.Extensions;

public static class HealthCheckExtensions
{
    private const string ReadyTag = "ready";

    public static IServiceCollection AddHealthCheck(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>(), name: "postgres", tags: [ReadyTag]);

        return services;
    }

    public static WebApplication MapHealthCheck(this WebApplication app)
    {
        // Liveness: a aplicação está no ar (não verifica dependências)
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteResponse
        });

        // Readiness: a aplicação e suas dependências (PostgreSQL) estão saudáveis
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadyTag),
            ResponseWriter = WriteResponse
        });

        return app;
    }

    private static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                error = entry.Value.Exception?.Message
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonSerializerOptions.Web));
    }
}
