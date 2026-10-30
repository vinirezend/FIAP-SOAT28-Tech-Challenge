namespace Oficina.Application.DTOs;

/// <summary>
/// Parâmetros para gerar um link assinado e enviá-lo por e-mail.
/// </summary>
public class EnvioLinkAssinadoDTO
{
    /// <summary>
    /// Caminho do endpoint liberado pelo link (ex.: /api/cliente/1/compartilhado), usado no placeholder {{link}}.
    /// Opcional quando LinksAssinados é informado.
    /// </summary>
    public string? Caminho { get; set; }

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
    /// Trechos de HTML inseridos sem codificação (ex.: linhas de uma tabela).
    /// Quem monta o trecho é responsável por codificar os valores vindos de dados.
    /// </summary>
    public IReadOnlyDictionary<string, string> ParametrosHtml { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Links assinados adicionais: placeholder → caminho (ex.: "aprovar" → /api/orcamento/1/aprovar).
    /// Todos usam a mesma validade.
    /// </summary>
    public IReadOnlyDictionary<string, string> LinksAssinados { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Quando não informado, usa o padrão da configuração.
    /// </summary>
    public int? MinutosValidade { get; set; }
}
