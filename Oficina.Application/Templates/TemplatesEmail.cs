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

    public const string OrcamentoAssunto = "Oficina - Orçamento da sua ordem de serviço";

    /// <summary>
    /// {{itens}} recebe as linhas da tabela já em HTML (TemplatesOrcamento.GerarLinhasItens);
    /// {{aprovar}} e {{recusar}} recebem os links assinados.
    /// </summary>
    public const string OrcamentoHtml = """
        <div style="font-family:Arial,sans-serif;color:#333;max-width:640px">
            <p>Olá, {{nome}}!</p>
            <p>O orçamento da ordem de serviço nº {{idOs}} para o veículo {{veiculo}} está pronto:</p>
            <table style="border-collapse:collapse;width:100%">
                <thead>
                    <tr style="background:#f2f2f2">
                        <th style="padding:8px;text-align:left">Tipo</th>
                        <th style="padding:8px;text-align:left">Descrição</th>
                        <th style="padding:8px;text-align:center">Qtd.</th>
                        <th style="padding:8px;text-align:right">Valor unit.</th>
                        <th style="padding:8px;text-align:right">Total</th>
                    </tr>
                </thead>
                <tbody>{{itens}}</tbody>
                <tfoot>
                    <tr>
                        <td colspan="4" style="padding:8px;text-align:right"><strong>Total do orçamento</strong></td>
                        <td style="padding:8px;text-align:right"><strong>{{valorTotal}}</strong></td>
                    </tr>
                </tfoot>
            </table>
            <p style="margin:24px 0">
                <a href="{{aprovar}}" style="background:#2e7d32;color:#fff;text-decoration:none;padding:12px 24px;border-radius:4px;display:inline-block;margin-right:12px">Aprovar orçamento</a>
                <a href="{{recusar}}" style="background:#c62828;color:#fff;text-decoration:none;padding:12px 24px;border-radius:4px;display:inline-block">Recusar orçamento</a>
            </p>
            <p>Os botões são válidos até {{expiraEm}} (horário de Brasília).</p>
        </div>
        """;

    public const string AcompanhamentoAssunto = "Oficina - Acompanhe sua ordem de serviço";

    public const string AcompanhamentoHtml = """
        <div style="font-family:Arial,sans-serif;color:#333;max-width:640px">
            <p>Olá, {{nome}}!</p>
            <p>Você pode acompanhar o andamento da ordem de serviço nº {{idOs}} do veículo {{veiculo}} pelo botão abaixo.</p>
            <p>Status atual: <strong>{{status}}</strong></p>
            <p style="margin:24px 0">
                <a href="{{link}}" style="background:#1565c0;color:#fff;text-decoration:none;padding:12px 24px;border-radius:4px;display:inline-block">Acompanhar ordem de serviço</a>
            </p>
            <p>Este link é válido até {{expiraEm}} (horário de Brasília).</p>
        </div>
        """;
}
