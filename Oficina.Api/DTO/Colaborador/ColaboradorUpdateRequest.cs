namespace Oficina.Api.DTO.Colaborador
{
    public class ColaboradorUpdateRequest
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public int IdDepartamento { get; set; }
        public int? IdUsuario { get; set; }
        public bool Ativo { get; set; }
    }
}
