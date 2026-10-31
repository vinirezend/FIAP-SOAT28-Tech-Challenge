using System.Security.Cryptography;
using Oficina.Application.Interfaces.Resources;

namespace Oficina.Infrastructure.Resources;

/// <summary>
/// Hash de senha com PBKDF2-SHA256 e salt aleatório, gravado como pbkdf2-sha256$iteracoes$salt$hash (Base64).
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const string Algoritmo = "pbkdf2-sha256";
    private const int Iteracoes = 100_000;
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;

    public string Gerar(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        return $"{Algoritmo}${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string senha, string senhaHash)
    {
        if (senhaHash.Split('$') is not [Algoritmo, var iteracoesTexto, var saltTexto, var hashTexto]
            || !int.TryParse(iteracoesTexto, out var iteracoes))
            return false;

        try
        {
            var salt = Convert.FromBase64String(saltTexto);
            var hashEsperado = Convert.FromBase64String(hashTexto);
            var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
