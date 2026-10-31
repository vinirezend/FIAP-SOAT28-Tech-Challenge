using System.ComponentModel.DataAnnotations;

namespace Oficina.Infrastructure.Configurations;

public class TokenConfiguration
{
    public const string SectionName = "TokenConfigurations";

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Chave HMAC-SHA256 de assinatura do JWT. Fora do Development deve vir de variável de ambiente (TokenConfigurations__Secret).
    /// </summary>
    [Required, MinLength(32)]
    public string Secret { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Minutes { get; set; } = 60;
}
