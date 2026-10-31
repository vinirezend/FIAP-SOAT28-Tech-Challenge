namespace Oficina.Application.Templates;

/// <summary>
/// Assuntos dos e-mails com link assinado. Os templates ficam em NomesTemplate e usam {{link}}, {{expiraEm}}
/// e as chaves informadas em EnvioLinkAssinadoDTO.Parametros.
/// </summary>
public static class TemplatesEmail
{
    public const string OrcamentoAssunto = "Oficina - Orçamento da sua ordem de serviço";

    public const string AcompanhamentoOrdemServicoAssunto = "Oficina - Acompanhe sua ordem de serviço";
}
