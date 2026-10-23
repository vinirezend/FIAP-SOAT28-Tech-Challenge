using Oficina.IntegrationTests.Infrastructure;
using Reqnroll;

namespace Oficina.IntegrationTests.Hooks;

[Binding]
public static class TestRunHooks
{
    public static PostgresDatabase Database { get; private set; } = null!;
    public static OficinaApiFactory Factory { get; private set; } = null!;

    [BeforeTestRun]
    public static async Task IniciarAmbienteAsync()
    {
        Database = new PostgresDatabase();
        await Database.StartAsync();

        Factory = new OficinaApiFactory(Database.ConnectionString);
    }

    [BeforeScenario]
    public static Task LimparBancoAsync() => Database.ResetAsync();

    [AfterTestRun]
    public static async Task FinalizarAmbienteAsync()
    {
        await Factory.DisposeAsync();
        await Database.DisposeAsync();
    }
}
