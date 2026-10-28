using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;
using Oficina.Application.Interfaces.Services;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace Oficina.Application.Implementations.Services;

public partial class EnvioLinkAssinadoService(ILinkAssinadoGenerator linkAssinadoGenerator, IEmailSender emailSender) : IEnvioLinkAssinadoService
{
    public const string PlaceholderLink = "link";
    public const string PlaceholderExpiraEm = "expiraEm";

    // Escapa < > & " ' mas mantém acentos legíveis no HTML (ex.: "João" em vez de "Jo&#227;o")
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);

    public async Task<LinkAssinadoDTO> EnviarAsync(EnvioLinkAssinadoDTO envio, CancellationToken cancellationToken)
    {
        var link = linkAssinadoGenerator.Gerar(envio.Caminho, envio.MinutosValidade);

        var parametros = new Dictionary<string, string>(envio.Parametros, StringComparer.OrdinalIgnoreCase)
        {
            [PlaceholderLink] = link.Url,
            [PlaceholderExpiraEm] = ConverterParaHorarioDeBrasilia(link.ExpiraEm).ToString("dd/MM/yyyy 'às' HH:mm")
        };

        var email = new EmailDTO
        {
            Assunto = envio.Assunto,
            EnderecoDestinatario = envio.EmailDestinatario,
            CorpoHtml = PreencherTemplate(envio.TemplateHtml, parametros)
        };

        await emailSender.EnviarAsync(email, cancellationToken);

        return link;
    }

    private static string PreencherTemplate(string template, Dictionary<string, string> parametros)
    {
        return Placeholder().Replace(template, match =>
        {
            var nome = match.Groups[1].Value;

            if (!parametros.TryGetValue(nome, out var valor))
                throw new InvalidOperationException($"O template de e-mail usa o placeholder {{{{{nome}}}}}, mas nenhum valor foi informado.");

            return Encoder.Encode(valor);
        });
    }

    private static DateTimeOffset ConverterParaHorarioDeBrasilia(DateTimeOffset data) =>
        TimeZoneInfo.TryFindSystemTimeZoneById("America/Sao_Paulo", out var fusoHorario)
            ? TimeZoneInfo.ConvertTime(data, fusoHorario)
            : data.ToOffset(TimeSpan.FromHours(-3));

    [GeneratedRegex(@"\{\{\s*(\w+)\s*\}\}")]
    private static partial Regex Placeholder();
}
