using System.Net.Http.Json;
using Oficina.IntegrationTests.Hooks;

namespace Oficina.IntegrationTests.Support;

/// <summary>
/// Estado HTTP de um cenário. O Reqnroll cria uma instância por cenário e a injeta nos steps.
/// </summary>
public class ApiContext
{
    public HttpClient Client { get; } = TestRunHooks.Factory.CreateClient(new()
    {
        BaseAddress = new Uri("https://localhost")
    });

    public HttpResponseMessage? LastResponse { get; private set; }

    public HttpResponseMessage Response =>
        LastResponse ?? throw new InvalidOperationException("Nenhuma requisição foi feita neste cenário.");

    public async Task PostAsync<T>(string url, T body) =>
        LastResponse = await Client.PostAsJsonAsync(url, body);

    public async Task PostAsync(string url, HttpContent content) =>
        LastResponse = await Client.PostAsync(url, content);
}
