using Oficina.Application.Common;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;
using Oficina.Application.Interfaces.Services;
using Oficina.Application.Templates;

namespace Oficina.Application.Implementations.Services;

public class EnvioLinkAssinadoService(ILinkAssinadoGenerator linkAssinadoGenerator, IEmailSender emailSender) : IEnvioLinkAssinadoService
{
    public const string PlaceholderLink = "link";
    public const string PlaceholderExpiraEm = "expiraEm";

    public async Task<LinkAssinadoDTO> EnviarAsync(EnvioLinkAssinadoDTO envio, CancellationToken cancellationToken)
    {
        var resultado = GerarLinks(envio);

        var parametros = new Dictionary<string, string>(envio.Parametros, StringComparer.OrdinalIgnoreCase)
        {
            [PlaceholderExpiraEm] = HorarioBrasilia.Converter(resultado.ExpiraEm).ToString("dd/MM/yyyy 'às' HH:mm")
        };
        if (!string.IsNullOrEmpty(resultado.Url))
            parametros[PlaceholderLink] = resultado.Url;
        foreach (var (placeholder, url) in resultado.Links)
            parametros[placeholder] = url;

        var email = new EmailDTO
        {
            Assunto = envio.Assunto,
            EnderecoDestinatario = envio.EmailDestinatario,
            CorpoHtml = TemplateHtml.Renderizar(envio.Template, parametros, envio.ParametrosHtml)
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
}
