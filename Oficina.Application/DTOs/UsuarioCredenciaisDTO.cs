using System.ComponentModel.DataAnnotations;

namespace Oficina.Application.DTOs
{
    public class UsuarioCredenciaisDTO
    {
        [Required] public string Login { get; set; } = string.Empty;

        [Required] public string Senha { get; set; } = string.Empty;
    }
}
