using System.Text.RegularExpressions;

namespace Oficina.Domain.ValueObjects
{
    public sealed record Placa
    {
            private static readonly Regex Formato = new(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", RegexOptions.Compiled);

    public string Numero { get; }

    private Placa(string numero) => Numero = numero;

    public static Placa Criar(string? valor)
    {
        var normalizada = (valor ?? "").ToUpperInvariant().Trim().Replace("-", "").Replace(" ", "");

        if (!Formato.IsMatch(normalizada))
            throw new ArgumentException("Placa inválida.");

        return new Placa(normalizada);
    }

    public override string ToString() => Numero;
    }
}