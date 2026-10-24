namespace Oficina.Domain.Models
{
    public class UsuarioAutenticacao
    {
        public int IdUsuario { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;

        public bool UsuarioAtivo { get; set; }

        public int? IdColaborador { get; set; }
        public string? Cpf { get; set; }
        public string? Email { get; set; }
        public int? IdDepartamento { get; set; }

        public bool? ColaboradorAtivo { get; set; }
    }
}
