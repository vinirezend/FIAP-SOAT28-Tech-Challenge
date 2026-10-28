namespace Oficina.Application.DTOs
{
    public class ColaboradorDTO
    {
        public int IdColaborador { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public bool Ativo { get; set; }
        public int IdDepartamento { get; set; }
        public int? IdUsuario { get; set; }
    }
}
