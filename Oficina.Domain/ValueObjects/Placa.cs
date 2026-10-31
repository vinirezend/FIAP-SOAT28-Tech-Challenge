using System.Text.RegularExpressions;

namespace Oficina.Domain.ValueObjects
{
    public sealed partial record Placa
    {
    [GeneratedRegex(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", RegexOptions.None, matchTimeoutMilliseconds: 100)]
    private static partial Regex Formato();

    public string Numero { get; }

    private Placa(string numero) => Numero = numero;

    public static Placa Criar(string? valor)
    {
        var normalizada = (valor ?? "").ToUpperInvariant().Trim().Replace("-", "").Replace(" ", "");

        if (!Formato().IsMatch(normalizada))
            throw new ArgumentException("Placa inválida.");

        return new Placa(normalizada);
    }

    public override string ToString() => Numero;
    }
}