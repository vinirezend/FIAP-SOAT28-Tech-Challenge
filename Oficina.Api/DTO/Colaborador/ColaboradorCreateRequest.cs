using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.Colaborador
{
    public class ColaboradorCreateRequest
    {
        public required string Nome { get; set; }
        public required string Cpf { get; set; }
        public required string Email { get; set; }
        [JsonRequired]
        public int IdDepartamento { get; set; }
        public int? IdUsuario { get; set; }
    }
}
