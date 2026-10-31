using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Oficina.IntegrationTests.Hooks;

namespace Oficina.IntegrationTests.Support;

/// <summary>
/// Estado HTTP de um cenário. O Reqnroll cria uma instância por cenário e a injeta nos steps.
/// </summary>
public class ApiContext
{
    public HttpClient Client { get; } = CriarClient();

    /// <summary>
    /// Navegador do cliente da oficina: abre os links assinados dos e-mails sem token JWT.
    /// </summary>
    public HttpClient ClienteAnonimo { get; } = CriarClient();

    public HttpResponseMessage? LastResponse { get; private set; }

    public HttpResponseMessage Response =>
        LastResponse ?? throw new InvalidOperationException("Nenhuma requisição foi feita neste cenário.");

    /// <summary>
    /// Corpo da última resposta, lido uma única vez para que mais de um step possa usá-lo.
    /// </summary>
    public string Corpo { get; private set; } = string.Empty;

    public void Autenticar(string token) =>
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public Task GetAsync(string url) =>
        GuardarAsync(Client.GetAsync(url));

    public Task PostAsync<T>(string url, T body) =>
        GuardarAsync(Client.PostAsJsonAsync(url, body));

    public Task PostAsync(string url, HttpContent? content = null) =>
        GuardarAsync(Client.PostAsync(url, content));

    public Task GetAnonimoAsync(string url) =>
        GuardarAsync(ClienteAnonimo.GetAsync(url));

    public Task PostAnonimoAsync(string url) =>
        GuardarAsync(ClienteAnonimo.PostAsync(url, null));

    /// <summary>
    /// Lê o corpo da última resposta, falhando com o conteúdo recebido quando o status não é de sucesso.
    /// </summary>
    public T Ler<T>()
    {
        if (!Response.IsSuccessStatusCode)
            Assert.Fail($"Status {(int)Response.StatusCode}: {Corpo}");

        return JsonSerializer.Deserialize<T>(Corpo, JsonSerializerOptions.Web)!;
    }

    private async Task GuardarAsync(Task<HttpResponseMessage> requisicao)
    {
        LastResponse = await requisicao;
        Corpo = await LastResponse.Content.ReadAsStringAsync();
    }

    private static HttpClient CriarClient() => TestRunHooks.Factory.CreateClient(new()
    {
        BaseAddress = new Uri("https://localhost")
    });
}
