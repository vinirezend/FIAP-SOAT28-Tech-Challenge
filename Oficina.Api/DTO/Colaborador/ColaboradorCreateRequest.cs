namespace Oficina.Api.DTO.Colaborador
{
    public class ColaboradorCreateRequest
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public int IdDepartamento { get; set; }
        public int? IdUsuario { get; set; }
    }
}
