using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

namespace Oficina.Api.Extensions;

public static class SwaggerExtensions
{
    private const string BearerScheme = "Bearer";

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            // Registra o esquema de autenticação JWT.
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Informe o token JWT."
                };

                return Task.CompletedTask;
            });

            // Aplica a autenticação (e documenta o 401) somente nos endpoints protegidos.
            options.AddOperationTransformer((operation, context, _) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                var requerAutenticacao = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();

                if (requerAutenticacao)
                {
                    operation.Security = [new() { [new OpenApiSecuritySchemeReference(BearerScheme, context.Document)] = [] }];

                    operation.Responses ??= new();
                    operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Não autenticado ou token expirado." });
                }

                return Task.CompletedTask;
            });
        });

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Oficina API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
