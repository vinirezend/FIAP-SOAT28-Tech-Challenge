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
        var resultado = GerarLinks(envio);

        var parametros = new Dictionary<string, string>(envio.Parametros, StringComparer.OrdinalIgnoreCase)
        {
            [PlaceholderExpiraEm] = ConverterParaHorarioDeBrasilia(resultado.ExpiraEm).ToString("dd/MM/yyyy 'às' HH:mm")
        };
        if (!string.IsNullOrEmpty(resultado.Url))
            parametros[PlaceholderLink] = resultado.Url;
        foreach (var (placeholder, url) in resultado.Links)
            parametros[placeholder] = url;

        var email = new EmailDTO
        {
            Assunto = envio.Assunto,
            EnderecoDestinatario = envio.EmailDestinatario,
            CorpoHtml = PreencherTemplate(envio.TemplateHtml, parametros, envio.ParametrosHtml)
        };

        await emailSender.EnviarAsync(email, cancellationToken);

        return resultado;
    }

    private LinkAssinadoDTO GerarLinks(EnvioLinkAssinadoDTO envio)
    {
        var principal = string.IsNullOrWhiteSpace(envio.Caminho)
            ? null
            : linkAssinadoGenerator.Gerar(envio.Caminho, envio.MinutosValidade);

        var links = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var expiraEm = principal?.ExpiraEm;
        foreach (var (placeholder, caminho) in envio.LinksAssinados)
        {
            var link = linkAssinadoGenerator.Gerar(caminho, envio.MinutosValidade);
            links[placeholder] = link.Url;
            expiraEm ??= link.ExpiraEm;
        }

        if (expiraEm is null)
            throw new InvalidOperationException("Informe o caminho de ao menos um link assinado.");

        return new LinkAssinadoDTO { Url = principal?.Url ?? string.Empty, ExpiraEm = expiraEm.Value, Links = links };
    }

    private static string PreencherTemplate(string template, Dictionary<string, string> parametros, IReadOnlyDictionary<string, string> parametrosHtml)
    {
        var html = new Dictionary<string, string>(parametrosHtml, StringComparer.OrdinalIgnoreCase);

        return Placeholder().Replace(template, match =>
        {
            var nome = match.Groups[1].Value;

            if (parametros.TryGetValue(nome, out var valor))
                return Encoder.Encode(valor);

            if (html.TryGetValue(nome, out var trechoHtml))
                return trechoHtml;

            throw new InvalidOperationException($"O template de e-mail usa o placeholder {{{{{nome}}}}}, mas nenhum valor foi informado.");
        });
    }

    private static DateTimeOffset ConverterParaHorarioDeBrasilia(DateTimeOffset data) =>
        TimeZoneInfo.TryFindSystemTimeZoneById("America/Sao_Paulo", out var fusoHorario)
            ? TimeZoneInfo.ConvertTime(data, fusoHorario)
            : data.ToOffset(TimeSpan.FromHours(-3));

    [GeneratedRegex(@"\{\{\s*(\w+)\s*\}\}")]
    private static partial Regex Placeholder();
}
