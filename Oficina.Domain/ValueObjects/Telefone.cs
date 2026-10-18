using System.Text.RegularExpressions;

namespace Oficina.Domain.ValueObjects;

public sealed record Telefone
{
    private static readonly Regex CaracteresPermitidos = new(@"^[\d\s\(\)\-]+$", RegexOptions.Compiled);

    public string Numero { get; }
    
    private Telefone(string numero) => Numero = numero;

    public static Telefone Criar(string? valor)
    {
        var bruto = (valor ?? "").Trim();

        if (string.IsNullOrWhiteSpace(bruto) || !CaracteresPermitidos.IsMatch(bruto))
            throw new ArgumentException("Telefone inválido.");

        var digitos = new string(bruto.Where(char.IsDigit).ToArray());

        if (digitos.Length < 8 || digitos.Length > 11)
            throw new ArgumentException("Telefone inválido.");

        return new Telefone(digitos);
    }

    public override string ToString() => Numero;
}