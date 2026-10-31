namespace Oficina.Application.Interfaces.Resources;

public interface IPasswordHasher
{
    /// <summary>
    /// Gera o hash da senha (com salt aleatório) para gravar no banco.
    /// </summary>
    string Gerar(string senha);

    /// <summary>
    /// Confere a senha informada com o hash gravado, em tempo constante.
    /// </summary>
    bool Verificar(string senha, string senhaHash);
}
