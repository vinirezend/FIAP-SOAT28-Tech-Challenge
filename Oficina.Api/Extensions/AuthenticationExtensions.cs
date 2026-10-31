using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Oficina.Infrastructure.Configurations;

namespace Oficina.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        // Segredo obrigatório e com tamanho mínimo: a API não sobe com a chave vazia
        services.AddOptions<TokenConfiguration>()
            .BindConfiguration(TokenConfiguration.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<TokenConfiguration>>().Value);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<TokenConfiguration>((options, token) =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = token.Issuer,
                    ValidAudience = token.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(token.Secret))
                };

                options.Events = new()
                {
                    OnChallenge = async context =>
                    {
                        // Impede a resposta padrão (401 sem corpo) e responde no mesmo formato de erro da API
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        await context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new()
                        {
                            HttpContext = context.HttpContext,
                            ProblemDetails =
                            {
                                Status = StatusCodes.Status401Unauthorized,
                                Title = "Não autenticado",
                                Detail = "Sua sessão expirou ou você não está autenticado."
                            }
                        });
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
