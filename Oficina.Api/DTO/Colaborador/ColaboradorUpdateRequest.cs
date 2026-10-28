using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.Colaborador
{
    public class ColaboradorUpdateRequest
    {
        public required string Nome { get; set; }
        public required string Email { get; set; }
        [JsonRequired]
        public int IdDepartamento { get; set; }
        public int? IdUsuario { get; set; }
        [JsonRequired]
        public bool Ativo { get; set; }
    }
}
