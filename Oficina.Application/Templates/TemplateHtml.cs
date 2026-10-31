using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace Oficina.Application.Templates;

/// <summary>
/// Carrega os templates HTML de Templates/Html (embutidos na DLL) e substitui os placeholders {{chave}}.
/// </summary>
public static partial class TemplateHtml
{
    // Escapa < > & " ' mas mantém acentos legíveis no HTML (ex.: "João" em vez de "Jo&#227;o")
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);

    private static readonly ConcurrentDictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);

    // O MSBuild monta o LogicalName com o separador do SO ("Templates/Orcamento\x.html" no Windows):
    // indexa os recursos com "/" para o nome do template ser o mesmo em qualquer ambiente.
    private static readonly Dictionary<string, string> Recursos = typeof(TemplateHtml).Assembly
        .GetManifestResourceNames()
        .Where(recurso => recurso.StartsWith("Templates/", StringComparison.Ordinal))
        .ToDictionary(recurso => recurso.Replace('\\', '/'), StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Renderiza o template Templates/Html/{nome}.html (ex.: NomesTemplate.Orcamento.Email = "Orcamento/email").
    /// </summary>
    /// <param name="valores">Chave sem as chaves duplas (ex.: "nome"); o valor é codificado em HTML.</param>
    /// <param name="trechosHtml">HTML inserido sem codificação, como o resultado de outro template renderizado.</param>
    public static string Renderizar(string nome, IReadOnlyDictionary<string, string> valores, IReadOnlyDictionary<string, string>? trechosHtml = null) =>
        Preencher(Carregar(nome), valores, trechosHtml);

    /// <summary>
    /// Renderiza o conteúdo da página e o insere no layout comum (Templates/Html/Comum/layout.html).
    /// </summary>
    public static string RenderizarPagina(string titulo, string nome, IReadOnlyDictionary<string, string> valores, IReadOnlyDictionary<string, string>? trechosHtml = null) =>
        Renderizar(NomesTemplate.Comum.Layout,
            new Dictionary<string, string> { ["titulo"] = titulo },
            new Dictionary<string, string> { ["conteudo"] = Renderizar(nome, valores, trechosHtml) });

    /// <summary>
    /// Substitui os placeholders {{chave}} (ignora espaços e maiúsculas/minúsculas).
    /// Lança InvalidOperationException se algum placeholder ficar sem valor.
    /// </summary>
    public static string Preencher(string html, IReadOnlyDictionary<string, string> valores, IReadOnlyDictionary<string, string>? trechosHtml = null)
    {
        var codificados = new Dictionary<string, string>(valores, StringComparer.OrdinalIgnoreCase);
        var trechos = trechosHtml is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(trechosHtml, StringComparer.OrdinalIgnoreCase);

        return Placeholder().Replace(html, match =>
        {
            var chave = match.Groups[1].Value;

            if (codificados.TryGetValue(chave, out var valor))
                return Encoder.Encode(valor);

            if (trechos.TryGetValue(chave, out var trecho))
                return trecho;

            throw new InvalidOperationException($"O template usa o placeholder {{{{{chave}}}}}, mas nenhum valor foi informado.");
        });
    }

    private static string Carregar(string nome) => Cache.GetOrAdd(nome, static nome =>
    {
        // LogicalName definido no Oficina.Application.csproj
        if (!Recursos.TryGetValue($"Templates/{nome}.html", out var nomeRecurso))
            throw new InvalidOperationException($"Template HTML '{nome}' não encontrado.");

        using var recurso = typeof(TemplateHtml).Assembly.GetManifestResourceStream(nomeRecurso)!;
        using var leitor = new StreamReader(recurso);
        return leitor.ReadToEnd();
    });

    [GeneratedRegex(@"\{\{\s*(\w+)\s*\}\}")]
    private static partial Regex Placeholder();
}
