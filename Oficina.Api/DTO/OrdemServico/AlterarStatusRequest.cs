using System.Text.Json.Serialization;

namespace Oficina.Api.DTO.OrdemServico
{
    public class AlterarStatusRequest
    {
        [JsonRequired]
        public int IdStatus { get; set; }
    }
}
