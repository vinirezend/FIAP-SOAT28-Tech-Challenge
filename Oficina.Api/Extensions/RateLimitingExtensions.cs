using System.Threading.RateLimiting;

namespace Oficina.Api.Extensions;

public static class RateLimitingExtensions
{
    /// <summary>
    /// Limita as tentativas de login por IP para dificultar ataques de força bruta.
    /// </summary>
    public const string PoliticaLogin = "login";

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var secao = configuration.GetSection("RateLimiting:Login");
        var limite = secao.GetValue("PermitLimit", 5);
        var janela = TimeSpan.FromSeconds(secao.GetValue("WindowSeconds", 60));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Atrás de proxy/load balancer o RemoteIpAddress é o do proxy: configure ForwardedHeaders nesse caso
            options.AddPolicy(PoliticaLogin, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = limite, Window = janela, QueueLimit = 0 }));

            options.OnRejected = (context, _) =>
                context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new()
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails =
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Muitas tentativas",
                        Detail = "Muitas tentativas de login. Aguarde um minuto e tente novamente."
                    }
                });
        });

        return services;
    }
}
