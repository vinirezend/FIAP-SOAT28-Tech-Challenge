namespace Oficina.Application.Templates;

/// <summary>
/// Templates HTML dos e-mails com link assinado. Usam {{link}}, {{expiraEm}}
/// e as chaves informadas em EnvioLinkAssinadoDTO.Parametros.
/// </summary>
public static class TemplatesEmail
{
    public const string DadosClienteAssunto = "Oficina - Acesse seus dados";

    public const string DadosClienteHtml = """
        <p>Olá, {{nome}}!</p>
        <p>Clique no link abaixo para visualizar seus dados:</p>
        <p><a href="{{link}}">{{link}}</a></p>
        <p>Este link é válido até {{expiraEm}} (horário de Brasília).</p>
        """;
}
