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
    }
}
