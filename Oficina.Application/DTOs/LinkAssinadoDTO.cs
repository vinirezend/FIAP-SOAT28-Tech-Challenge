namespace Oficina.Application.DTOs;

public class LinkAssinadoDTO
{
    /// <summary>
    /// Link de EnvioLinkAssinadoDTO.Caminho; vazio quando só LinksAssinados é informado.
    /// </summary>
    public string Url { get; set; }
    public DateTimeOffset ExpiraEm { get; set; }

    /// <summary>
    /// Links gerados a partir de EnvioLinkAssinadoDTO.LinksAssinados (placeholder → URL).
    /// </summary>
    public IReadOnlyDictionary<string, string> Links { get; set; } = new Dictionary<string, string>();
}
