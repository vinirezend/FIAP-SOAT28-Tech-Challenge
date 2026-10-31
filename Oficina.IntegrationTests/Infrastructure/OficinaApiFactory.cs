using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Oficina.IntegrationTests.Infrastructure;

/// <summary>
/// Sobe a API em memória (TestServer) apontando para o PostgreSQL do Testcontainers.
/// </summary>
public class OficinaApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" evita carregar o appsettings.Development.json (que aponta para localhost:5432)
        builder.UseEnvironment("Testing");

        // UseSetting (e não ConfigureAppConfiguration) porque o Program.cs lê a
        // connection string antes do builder.Build()
        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);

        // Credenciais fictícias para passar na validação do EmailConfiguration (ValidateOnStart)
        builder.UseSetting("EmailConfigurations:User", "usuario-teste");
        builder.UseSetting("EmailConfigurations:Key", "chave-teste");

        // Segredos não ficam no appsettings.json; o ambiente "Testing" precisa informá-los
        builder.UseSetting("TokenConfigurations:Secret", "chave-jwt-de-teste-com-tamanho-suficiente");
        builder.UseSetting("LinkAssinadoConfigurations:Secret", "chave-link-de-teste-com-tamanho-suficiente");

        // Os cenários fazem vários logins seguidos do mesmo "IP"; o limite de produção (5/min) quebraria os testes
        builder.UseSetting("RateLimiting:Login:PermitLimit", "1000");
    }
}
