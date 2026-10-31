using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Oficina.Api.Conventions;
using Oficina.Api.ErrorHandling;
using Oficina.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRepositories()
    .AddApplicationServices()
    .AddResources()
    .AddJwtAuthentication()
    .AddRateLimiting(builder.Configuration)
    .AddDatabase(builder.Configuration)
    .AddHealthCheck()
    .AddSwaggerDocumentation();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));

    // Mantém o sufixo "Async" no nome da action para nameof(...) funcionar no Url.Action
    options.SuppressAsyncSuffixInActionNames = false;

    // Erros tratados pelo GlobalExceptionHandler, documentados em todos os endpoints
    options.Filters.Add(new ProducesResponseTypeAttribute<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json"));
    options.Filters.Add(new ProducesResponseTypeAttribute<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json"));
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwaggerDocumentation();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthCheck();

await app.RunAsync();
