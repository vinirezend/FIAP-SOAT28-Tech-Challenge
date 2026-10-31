using Oficina.Infrastructure.Resources;

namespace Oficina.Tests.Infraestructure.Resources;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Verificar_ComSenhaCorreta_DeveRetornarTrue()
    {
        var hash = _hasher.Gerar("senha123");

        Assert.True(_hasher.Verificar("senha123", hash));
    }

    [Fact]
    public void Verificar_ComSenhaIncorreta_DeveRetornarFalse()
    {
        var hash = _hasher.Gerar("senha123");

        Assert.False(_hasher.Verificar("senha-errada", hash));
    }

    [Fact]
    public void Gerar_MesmaSenha_DeveGerarHashesDiferentes()
    {
        Assert.NotEqual(_hasher.Gerar("senha123"), _hasher.Gerar("senha123"));
    }

    [Fact]
    public void Gerar_NaoDeveConterASenha()
    {
        var hash = _hasher.Gerar("senha123");

        Assert.StartsWith("pbkdf2-sha256$100000$", hash);
        Assert.DoesNotContain("senha123", hash);
    }

    [Theory]
    [InlineData("senha123")]
    [InlineData("")]
    [InlineData("pbkdf2-sha256$abc$salt$hash")]
    [InlineData("pbkdf2-sha256$100000$!!invalido!!$hash")]
    [InlineData("outro-algoritmo$100000$c2FsdA==$aGFzaA==")]
    public void Verificar_ComHashEmFormatoInvalido_DeveRetornarFalse(string hashInvalido)
    {
        Assert.False(_hasher.Verificar("senha123", hashInvalido));
    }

    [Fact]
    public void Verificar_HashGeradoForaDaAplicacao_DeveSerAceito()
    {
        // Mesmo formato usado no seed (database/init/013_insert_inicial.sql), gerado com hashlib.pbkdf2_hmac
        const string hashSeed = "pbkdf2-sha256$100000$aSkxI5NOEpvJDGkntot+fg==$m8xjOeVrrPgjX8lngRivlLpd7L6bAMQVD/+LTDt2btY=";

        Assert.True(_hasher.Verificar("gerente123!", hashSeed));
    }
}
