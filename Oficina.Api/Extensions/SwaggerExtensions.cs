using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

namespace Oficina.Api.Extensions;

public static class SwaggerExtensions
{
    private const string BearerScheme = "Bearer";

    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            // Registra o esquema de autenticação JWT.
            options.AddDocumentTransformer(
                (document, context, cancellationToken) =>
                {
                    document.Components ??= new OpenApiComponents();

                    document.Components.SecuritySchemes ??=
                        new Dictionary<string, IOpenApiSecurityScheme>();

                    document.Components.SecuritySchemes[BearerScheme] =
                        new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                            Description = "Informe o token JWT."
                        };

                    return Task.CompletedTask;
                });

            // Aplica a autenticação somente aos endpoints protegidos.
            options.AddOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    var metadata =
                        context.Description.ActionDescriptor.EndpointMetadata;

                    var requiresAuthorization =
                        metadata.OfType<IAuthorizeData>().Any();

                    var allowsAnonymous =
                        metadata.OfType<IAllowAnonymous>().Any();

                    if (requiresAuthorization && !allowsAnonymous)
                    {
                        operation.Security =
                        [
                            new OpenApiSecurityRequirement
                            {
                                {
                                    new OpenApiSecuritySchemeReference(BearerScheme, context.Document),
                                    new List<string>()
                                }
                            }
                        ];
                    }

                    return Task.CompletedTask;
                });
        });

        return services;
    }

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
