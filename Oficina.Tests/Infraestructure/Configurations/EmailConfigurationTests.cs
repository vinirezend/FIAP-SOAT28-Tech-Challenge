using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oficina.Infrastructure.Configurations;

namespace Oficina.Tests.Infraestructure.Configurations;

public class EmailConfigurationTests
{
    private static Dictionary<string, string?> CriarConfiguracaoValida()
    {
        return new Dictionary<string, string?>
        {
            [$"{EmailConfiguration.SectionName}:Host"] = "smtp.teste.com",
            [$"{EmailConfiguration.SectionName}:Port"] = "587",
            [$"{EmailConfiguration.SectionName}:User"] = "usuario-teste",
            [$"{EmailConfiguration.SectionName}:Key"] = "chave-teste",
            [$"{EmailConfiguration.SectionName}:Address"] = "remetente@teste.com",
            [$"{EmailConfiguration.SectionName}:AddressName"] = "Oficina Teste"
        };
    }

    // Mesmo registro usado no Program.cs
    private static EmailConfiguration ObterConfiguracao(Dictionary<string, string?> valores)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(valores)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddOptions<EmailConfiguration>()
            .BindConfiguration(EmailConfiguration.SectionName)
            .ValidateDataAnnotations();

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<EmailConfiguration>>().Value;
    }

    [Fact]
    public void Bind_ComConfiguracaoValida_DevePreencherTodasAsPropriedades()
    {
        // Act
        var resultado = ObterConfiguracao(CriarConfiguracaoValida());

        // Assert
        Assert.Equal("smtp.teste.com", resultado.Host);
        Assert.Equal(587, resultado.Port);
        Assert.Equal("usuario-teste", resultado.User);
        Assert.Equal("chave-teste", resultado.Key);
        Assert.Equal("remetente@teste.com", resultado.Address);
        Assert.Equal("Oficina Teste", resultado.AddressName);
    }

    [Theory]
    [InlineData("Host")]
    [InlineData("User")]
    [InlineData("Key")]
    [InlineData("Address")]
    public void Bind_SemCampoObrigatorio_DeveLancarOptionsValidationException(string campo)
    {
        // Arrange
        var valores = CriarConfiguracaoValida();
        valores.Remove($"{EmailConfiguration.SectionName}:{campo}");

        // Act & Assert
        var excecao = Assert.Throws<OptionsValidationException>(
            () => ObterConfiguracao(valores));

        Assert.Contains(campo, excecao.Message);
    }

    [Fact]
    public void Bind_ComAddressInvalido_DeveLancarOptionsValidationException()
    {
        // Arrange
        var valores = CriarConfiguracaoValida();
        valores[$"{EmailConfiguration.SectionName}:Address"] = "email-invalido";

        // Act & Assert
        var excecao = Assert.Throws<OptionsValidationException>(
            () => ObterConfiguracao(valores));

        Assert.Contains("Address", excecao.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("65536")]
    public void Bind_ComPortaForaDoIntervalo_DeveLancarOptionsValidationException(string porta)
    {
        // Arrange
        var valores = CriarConfiguracaoValida();
        valores[$"{EmailConfiguration.SectionName}:Port"] = porta;

        // Act & Assert
        var excecao = Assert.Throws<OptionsValidationException>(
            () => ObterConfiguracao(valores));

        Assert.Contains("Port", excecao.Message);
    }

    [Fact]
    public void Bind_SemAddressName_DeveSerValido()
    {
        // Arrange
        var valores = CriarConfiguracaoValida();
        valores.Remove($"{EmailConfiguration.SectionName}:AddressName");

        // Act
        var resultado = ObterConfiguracao(valores);

        // Assert
        Assert.Equal(string.Empty, resultado.AddressName);
    }
}
