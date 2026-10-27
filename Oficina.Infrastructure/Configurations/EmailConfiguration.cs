using System.ComponentModel.DataAnnotations;

namespace Oficina.Infrastructure.Configurations
{
    public class EmailConfiguration
    {
        public const string SectionName = "EmailConfigurations";

        [Required]
        public string Host { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int Port { get; set; }

        [Required]
        public string User { get; set; } = string.Empty;

        [Required]
        public string Key { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Address { get; set; } = string.Empty;

        public string AddressName { get; set; } = string.Empty;
    }
}
