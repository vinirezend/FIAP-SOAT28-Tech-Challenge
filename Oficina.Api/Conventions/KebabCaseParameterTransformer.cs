using System.Text.RegularExpressions;

namespace Oficina.Api.Conventions;

/// <summary>
/// Converte os tokens de rota ([controller], [action]) para kebab-case: OrdemServico → ordem-servico.
/// </summary>
public partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value?.ToString() is not { Length: > 0 } texto)
            return null;

        return PalavraRegex().Replace(texto, "$1-$2").ToLowerInvariant();
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex PalavraRegex();
}
