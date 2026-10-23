using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Oficina.Domain.Configurations;
using Oficina.IntegrationTests.Hooks;
using Oficina.IntegrationTests.Support;
using Reqnroll;

namespace Oficina.IntegrationTests.Steps;

[Binding]
public class LoginSteps(ApiContext api)
{
    private const string SignInUrl = "/api/login/signin";

    [Given(@"que existe o usuário ""(.*)"" com senha ""(.*)""")]
    public static Task DadoQueExisteOUsuario(string login, string senha) =>
        UsuarioSeeder.CriarAsync(login, senha);

    [When(@"eu fizer login com usuário ""(.*)"" e senha ""(.*)""")]
    public Task QuandoEuFizerLogin(string login, string senha) =>
        api.PostAsync(SignInUrl, new { login, senha });

    [When(@"eu fizer login sem enviar o corpo da requisição")]
    public Task QuandoEuFizerLoginSemCorpo()
    {
        var conteudoVazio = new StringContent(string.Empty);
        conteudoVazio.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return api.PostAsync(SignInUrl, conteudoVazio);
    }

    [Then(@"devo receber o status (\d+)")]
    public void EntaoDevoReceberOStatus(int status) =>
        Assert.Equal(status, (int)api.Response.StatusCode);

    [Then(@"a resposta deve conter um token JWT válido para o usuário ""(.*)""")]
    public async Task EntaoARespostaDeveConterUmTokenValido(string login)
    {
        var token = (await api.Response.Content.ReadAsStringAsync()).Trim('"');
        var configuracao = TestRunHooks.Factory.Services.GetRequiredService<TokenConfiguration>();

        // MapInboundClaims = false mantém os nomes originais das claims (ex.: "unique_name")
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

        var resultado = await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = configuracao.Issuer,
            ValidAudience = configuracao.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracao.Secret))
        });

        Assert.True(resultado.IsValid, resultado.Exception?.Message);
        Assert.Equal(login, resultado.Claims[JwtRegisteredClaimNames.UniqueName]);
    }
}
