using System.ComponentModel.DataAnnotations;

namespace Oficina.Infrastructure.Configurations
{
    public class LinkAssinadoConfiguration
    {
        public const string SectionName = "LinkAssinadoConfigurations";

        [Required, MinLength(32)]
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// URL pública da API usada para montar o link (ex.: https://api.oficina.com.br).
        /// Quando vazia, usa o host da requisição atual.
        /// </summary>
        public string UrlBase { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int MinutosValidadePadrao { get; set; } = 1440;

        [Range(1, int.MaxValue)]
        public int MinutosValidadeMaxima { get; set; } = 10080;
    }
}
