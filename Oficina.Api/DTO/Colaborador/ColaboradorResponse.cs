namespace Oficina.Api.DTO.Colaborador
{
    public class ColaboradorResponse
    {
        public int IdColaborador { get; set; }
        public required string Nome { get; set; }
        public required string Cpf { get; set; }
        public required string Email { get; set; }
        public bool Ativo { get; set; }
        public int IdDepartamento { get; set; }
        public int? IdUsuario { get; set; }
    }
}
