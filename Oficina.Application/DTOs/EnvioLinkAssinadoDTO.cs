namespace Oficina.Application.DTOs;

/// <summary>
/// Parâmetros para gerar um link assinado e enviá-lo por e-mail.
/// </summary>
public class EnvioLinkAssinadoDTO
{
    /// <summary>
    /// Caminho do endpoint liberado pelo link (ex.: /api/cliente/1/compartilhado).
    /// </summary>
    public string Caminho { get; set; }

    public string EmailDestinatario { get; set; }

    public string Assunto { get; set; }

    /// <summary>
    /// HTML do e-mail com placeholders no formato {{chave}}.
    /// {{link}} e {{expiraEm}} são preenchidos automaticamente.
    /// </summary>
    public string TemplateHtml { get; set; }

    /// <summary>
    /// Valores que substituem os placeholders do template (chave sem as chaves duplas, ex.: "nome").
    /// São codificados em HTML antes de entrar no corpo do e-mail.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parametros { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Quando não informado, usa o padrão da configuração.
    /// </summary>
    public int? MinutosValidade { get; set; }
}
