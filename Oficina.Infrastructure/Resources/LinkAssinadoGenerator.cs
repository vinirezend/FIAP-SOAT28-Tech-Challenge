using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces.Resources;
using Oficina.Infrastructure.Configurations;
using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Oficina.Infrastructure.Resources;

/// <summary>
/// Gera e valida links pré-assinados: a URL carrega a data de expiração e uma assinatura
/// HMAC-SHA256 de (caminho + expiração). Qualquer alteração no caminho ou na expiração invalida o link.
/// </summary>
public class LinkAssinadoGenerator(
    IOptions<LinkAssinadoConfiguration> options,
    IHttpContextAccessor httpContextAccessor,
    TimeProvider timeProvider) : ILinkAssinadoGenerator
{
    private readonly LinkAssinadoConfiguration _configuration = options.Value;

    public LinkAssinadoDTO Gerar(string caminho, int? minutosValidade = null)
    {
        var minutos = Math.Clamp(
            minutosValidade ?? _configuration.MinutosValidadePadrao,
            1,
            _configuration.MinutosValidadeMaxima);

        var expiraEm = timeProvider.GetUtcNow().AddMinutes(minutos).ToUnixTimeSeconds();
        var caminhoNormalizado = Normalizar(caminho);
        var assinatura = Base64Url.EncodeToString(Assinar(caminhoNormalizado, expiraEm));

        return new LinkAssinadoDTO
        {
            Url = $"{ObterUrlBase()}{caminhoNormalizado}" +
                  $"?{ParametrosLinkAssinado.Expira}={expiraEm}&{ParametrosLinkAssinado.Assinatura}={assinatura}",
            ExpiraEm = DateTimeOffset.FromUnixTimeSeconds(expiraEm)
        };
    }

    public StatusLinkAssinado Validar(string caminho, string? expira, string? assinatura)
    {
        if (string.IsNullOrWhiteSpace(assinatura) ||
            !long.TryParse(expira, NumberStyles.None, CultureInfo.InvariantCulture, out var expiraEm))
            return StatusLinkAssinado.Invalido;

        byte[] assinaturaRecebida;
        try
        {
            assinaturaRecebida = Base64Url.DecodeFromChars(assinatura);
        }
        catch (FormatException)
        {
            return StatusLinkAssinado.Invalido;
        }

        var assinaturaEsperada = Assinar(Normalizar(caminho), expiraEm);

        // Comparação em tempo constante para não vazar informação sobre a assinatura
        if (!CryptographicOperations.FixedTimeEquals(assinaturaRecebida, assinaturaEsperada))
            return StatusLinkAssinado.Invalido;

        return timeProvider.GetUtcNow().ToUnixTimeSeconds() > expiraEm
            ? StatusLinkAssinado.Expirado
            : StatusLinkAssinado.Valido;
    }

    private byte[] Assinar(string caminho, long expiraEm)
    {
        var chave = Encoding.UTF8.GetBytes(_configuration.Secret);
        var conteudo = Encoding.UTF8.GetBytes($"{caminho}|{expiraEm}");
        return HMACSHA256.HashData(chave, conteudo);
    }

    // Rotas do ASP.NET não diferenciam maiúsculas/minúsculas nem barra final
    private static string Normalizar(string caminho)
    {
        var normalizado = caminho.Trim().TrimEnd('/').ToLowerInvariant();
        return normalizado.StartsWith('/') ? normalizado : $"/{normalizado}";
    }

    private string ObterUrlBase()
    {
        if (!string.IsNullOrWhiteSpace(_configuration.UrlBase))
            return _configuration.UrlBase.TrimEnd('/');

        var request = httpContextAccessor.HttpContext?.Request
            ?? throw new InvalidOperationException(
                $"Configure {LinkAssinadoConfiguration.SectionName}:UrlBase para gerar links fora de uma requisição HTTP.");

        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }
}
